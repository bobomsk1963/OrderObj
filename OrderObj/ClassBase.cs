using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Data.SqlServerCe;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
namespace OrderObj
{
    // Работа с базой MsSqlCe
    // База создается если ее нет в каталоге запуска программы

    public 
        interface IData
    {
        bool ThisToRow(DataRow dr);
        bool RowToThis(DataRow dr);
    }

    public class ClassBase
    {

        public string FileName = "";
        public SqlCeCommand DBase = null;
        public bool IsOpen = false;

        public DataSet dataSet = null;

        string connString
        {
            get;
            set;
        }

        public ClassBase(string filename, string endconnString)
        {
            FileName = filename;
            connString = "Data Source=" + FileName + "; " + endconnString;
        }

        public bool Create()
        {
            bool ret = true;
            // Проверяем если файла нет в наличии то создаем базу данных
            if (!File.Exists(FileName))
            {
                //Создание файла базы данных !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                SqlCeEngine engine = new SqlCeEngine();

                try
                {
                    engine.LocalConnectionString = connString;
                    engine.CreateDatabase();
                }
                catch
                {
                    ret = false;
                }
                finally
                {
                    if (engine != null) { engine.Dispose(); }
                }
            }
            return ret;
        }


        public bool Open()
        {
            // Открытие файла базы данных
            bool ret = false;
            if (File.Exists(FileName))
            {
                try
                {
                    DBase = new SqlCeCommand();
                    DBase.Connection = new SqlCeConnection(connString);
                    DBase.Connection.Open();

                    dataSet = new DataSet(); // Для ADO

                    IsOpen = true;
                    ret = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Внимание!");
                }
            }
            else
            {
                MessageBox.Show("Файла нет в наличии!", "Внимание!");
            }
            return ret;
        }

        public void Close()
        {
            // Закрытие файла базы данных
            try
            {
                DBase.Connection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Внимание!");
            }
            IsOpen = false;
        }

        public bool TestTable(string TableName)
        // проверить существование таблиц
        {
            // Проверка наличия таблицы в файле базы данных
            bool ret = false;
            try
            {
                DBase.CommandText = "Select * From " + TableName + " Where Id=0;";
                using (SqlCeDataReader rdr = DBase.ExecuteReader())
                {
                    rdr.Close();
                    ret = true;
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("Тест - " + TableName +" - "+ e.Message, "Внимание!");
            }
            return ret;
        }

        public bool CreateTable(string Table)
        {
            //Создание талиц базы данных
            bool ret = false;
            try
            {
                DBase.CommandText = Table;
                DBase.ExecuteNonQuery();
                ret = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Внимание!");
            }
            return ret;
        }

        public bool CreateIndex(string TableName, string Pole, int NumberIndex)
        {
            //Создание индексов таблиц
            bool ret = false;
            try
            {
                DBase.CommandText = "CREATE INDEX [Idx_" + TableName + "_" + NumberIndex.ToString() + "] ON [" + TableName + "] ([" + Pole + "] ASC);";
                DBase.ExecuteNonQuery();
                ret = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Внимание!");
            }
            return ret;
        }

        public bool DelTable(string TableName)
        {
            // Удаление таблмц ;
            bool ret = false;
            try
            {
                DBase.CommandText = "DROP TABLE " + TableName + "; \r\n";
                int i = DBase.ExecuteNonQuery();
                ret = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Внимание!");
            }
            return ret;
        }
    }

    // Базовый класс для таблиц базы данных
    public class ClassTable
    {
        public ClassBase classBase = null;
        public SqlCeCommand DBase;
        public string TableName = "";
        string createTableStr = "";
        public string CreateTableStr
        {
            get { return "Create Table " + TableName + " " + createTableStr; }
            set { createTableStr = value; }
        }

        public ClassTable(ClassBase classbase, string tablename, string createtablestr)
        {
            classBase = classbase;
            DBase = classBase.DBase;
            TableName = tablename.Trim();
            CreateTableStr = createtablestr;
            Param();
        }

        public virtual void Param()
        {
        }

        public virtual bool CreateTable()
        {
            bool ret = false;
            if (classBase.CreateTable(CreateTableStr))
            {
                ret = CreateIndex();
            }
            return ret;
        }

        public virtual bool CreateIndex()
        {
            return true;
        }

        public virtual void Load()//DataSet ds=null)
        {
        }

        public bool Open()
        {
            bool ret = false;

            if (classBase.TestTable(TableName))
            {
                ret = true;
            }
            else
            {
                if (!CreateTable())
                {
                    MessageBox.Show("Таблица " + TableName + " не создана!", "Внимание!");
                }
                else { ret = true; }// загружать нечего таблица создана но пустая
            }
            return ret;
        }


    }

    public enum SortOrientation { Desc, Asc }

    public class ClassTableAdo : ClassTable
    {

        public DataView dataView = null;
        public DataTable dataTable = null;


        bool SetDataViewOnLoad = true;
        public DataRelation dataRelation = null;

        public SqlCeDataAdapter Adapter = null;
        SqlCeCommandBuilder commandBuilder = null;
        int sortIndex = 0;
        SortOrientation sortorientation = SortOrientation.Asc;

        public int SortIndex
        {
            get { return sortIndex; }
            set
            {
                try
                {
                    string s = dataView.Table.Columns[value].Caption;
                    if (sortOrientation == SortOrientation.Asc)
                    {
                        dataView.Sort = s + " ASC";
                    }
                    else
                    {
                        dataView.Sort = s + " DESC";
                    }
                    sortIndex = value;

                }
                catch { };
            }
        }

        public SortOrientation sortOrientation
        {
            get { return sortorientation; }
            set { sortorientation = value; }
        }

        public ClassTableAdo(ClassBase classbase, string tablename, string createtablestr, bool setdataviewonload = true)
            : base(classbase, tablename, createtablestr)
        {
            SetDataViewOnLoad = setdataviewonload;
        }

        public bool readRow(int index, IData obj)
        {
            bool ret = true;
            try
            {
                obj.RowToThis(dataView[index].Row);
            }
            catch { ret = false; }
            return ret;
        }

        public int changeRow(int index, IData obj)
        {
            int ret = 0;
            try
            {
                DataRowView dr = dataView[index];
                dr.BeginEdit();
                obj.ThisToRow(dr.Row);
                dr.EndEdit();
                ret = Adapter.Update(dataTable);
                // Поиск измененного номера записи в dataView по индексному полю
            }
            catch (Exception e)
            { }
            return ret; // Возвращает новое положение или -1 если ошибка
        }

        public bool delRow(int index, bool update = true)
        {
            bool ret = false;
            try
            {
                dataView[index].Delete();
                if (update)
                {
                    Adapter.Update(dataTable);
                }
                //ret = n > 0;
                ret = true;
            }
            catch (Exception e)
            {
            }
            return ret;
        }

        // На выходе индекс добавленной строки в DataSet или -1

        public int addRow(IData obj)//params object[] d) 
        {
            int n = -1;
            try
            {
                DataRowView dr = dataView.AddNew();
                obj.ThisToRow(dr.Row);
                dr.EndEdit();
                n = Adapter.Update(dataTable);
                if (n > 0)
                {
                    //n = dataView.Find(dr[sortIndex]);  // Значение индекса сортировки
                    // Поиск измененного номера записи в dataView по индексному полю
                }
            }
            catch { }
            return n;
        }


        public override bool CreateIndex()
        {
            bool ret = true;//false;
            //ret = classBase.CreateIndex(TableName, "Id", 0);
            return ret;
        }

        public override void Load()//DataSet ds=null)  // Параметром передовать датасет 
        {
            if (Adapter == null)
            {

                string sql = "SELECT * FROM " + TableName + ";";//+ " ORDER BY Id;";
                Adapter = new SqlCeDataAdapter(sql, DBase.Connection);

                // Для commandBuilder необходим хотябы один первичный ключ
                // столбец первичного ключа или столбец с атрибутом UNIQUE

                commandBuilder = new SqlCeCommandBuilder();
                commandBuilder.DataAdapter = Adapter;

                Adapter.UpdateBatchSize = 1;

                Adapter.RowUpdating += new SqlCeRowUpdatingEventHandler(RowUpdating);
                Adapter.RowUpdated += new SqlCeRowUpdatedEventHandler(RowUpdated);


            }
            else
            {
                //dataSet.Clear();
            }

            Adapter.Fill(classBase.dataSet);

            dataTable = classBase.dataSet.Tables["Table"];
            dataTable.TableName = TableName;

            if (SetDataViewOnLoad)
            {
                SetDataView(null);
            }
            //dataView = new DataView(dataTable);//dataSet.Tables[0]);
            //SortIndex = sortIndex;

        }



        void SetDataView(DataRowView drv = null)
        {
            if ((dataRelation != null) && (drv != null))
            {
                dataView = drv.CreateChildView(dataRelation);
            }
            else
            {
                dataView = new DataView(dataTable);
            }
            SortIndex = sortIndex;
        }



        public void UpdateTable()
        {
            Adapter.Update(dataTable);
        }

        void RowUpdating(Object sender, SqlCeRowUpdatingEventArgs e)
        {
            //string ss = e.Row.Table.TableName;

            //StatementType.

            if (e.StatementType == StatementType.Insert)
            {
                // Начало транзакции
                e.Command.Transaction = e.Command.Connection.BeginTransaction();

            }

            //if (e.StatementType == StatementType.Delete)
            //{
            //    string s = e.Row.Table.TableName;
            //}

            AdditionRowUpdating(sender, e);
        }

        public virtual void AdditionRowUpdating(Object sender, SqlCeRowUpdatingEventArgs e)
        {
        }

        void RowUpdated(Object sender, SqlCeRowUpdatedEventArgs e)
        {
            if (e.Errors == null)
            {
                if (e.StatementType == StatementType.Insert)
                {
                    try
                    {
                        e.Command.CommandText = "SELECT @@IDENTITY;";
                        e.Row[0] = e.Command.ExecuteScalar();
                        e.Command.Transaction.Commit();
                        e.Row.AcceptChanges();
                    }
                    catch
                    { e.Command.Transaction.Rollback(); }
                }
            }
            else
            {
                e.Command.Transaction.Rollback();
                MessageBox.Show(e.Errors.ToString());
            }
            e.Command.Transaction = null;
        }

    }

    //*******************************************************************************


    public class ClassOrder : ClassTableAdo
    {
        public ClassOrder(ClassBase classbase)
            : base(classbase, "TableOrder", CreateStr.CreateOrderStr)
        {
        }

    }

    // Создание или открытие базы и таблиц данных
    public class ClassOpenBase
    {
        public ClassOrder classOrder;



        public ClassBase Base = null;
        public ClassOpenBase()
        {
            //  Создание или открытие базы и таблиц данных
            Base = new ClassBase("OrderBaseObj.sdf", "Max Database Size = 4090; Mode = Read Write; Max Buffer Size = 10240;");
            if (Base.Create())
            {
                if (Base.Open())
                {
                    classOrder = new ClassOrder(Base);
                    if (classOrder.Open())
                    {
                        classOrder.Load();
                    }

                }
                else { MessageBox.Show("Ошибка открытия базы!", "Внимание!"); }
            }
            else { MessageBox.Show("Ошибка создания базы!", "Внимание!"); }

        }
    }

}
