using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
namespace OrderObj
{
    static class CreateStr  // 
    {
        public readonly static string CreateOrderStr =
                        "(" +
                        "[Id] BIGINT NOT NULL IDENTITY (1,1) PRIMARY KEY," +           // uniqueidentifier
                        "[NDock] NVARCHAR(20) NOT NULL," +                             // Номер документа
                        "[Summ] REAL NOT NULL," +                                      // Сумма заказа
                        "[Status] INT NOT NULL," +                                     // Status заказа
                        "[Comment] NVARCHAR(250) NOT NULL,"+                           // Коментарий
                        "[NumbTypeOrder] INT NOT NULL," +
                        "[ExtData] IMAGE);";
    }


    // В структуре классов применен Паттеон - Strategy
    // class cOrder - основной класс
    // interface iOrder - интерфейс функциональностей
    // class cOrderVendor - конкретные реализации заказов
    // class cOrderVendor -  
    //
    // Для добавления нового вида заказа надо создать класс с интерфейсом interface iOrder и добавить этот класс в массив для создания   - ypeCreater[] masTypeOrder
    // В это классе должен быть реализован кнструктор со строковым параметром имени класаа
    // поле в котором буддет сохраняться имя класса и переопределить процедуру ToStrng();

// class Заказа
   class cOrder : IData
    {
        public static string[] statusmas = new string[2] { "Ожидает обработки", "Обработано" };

        public cOrder()
        {
            NDock = "";
            Summ = 0; 
            Status = 0; 
            NumbTypeOrder = -1;
            Comment = "";

            ExtDataOrder = null;
        }

        public cOrder(DataRow dr)
        {
            RowToThis(dr);
        }

       // Сохранение данных из структуры в запись базы данных
        public bool ThisToRow(DataRow dr)
        {
            bool ret = false;
            try
            {
                dr["NDock"] = NDock;
                dr["Summ"] = Summ;
                dr["Status"] = Status;
                dr["NumbTypeOrder"] = NumbTypeOrder;
                dr["Comment"] = Comment;


                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    formatter.Serialize(ms, extDataOrder);
                    byte[] mb = ms.ToArray();
                    dr["ExtData"] = mb;
                }

                ret = true;
            }
            catch { }
            return ret;
        }

       // Загрузка данных и записи в структуру
        public bool RowToThis(DataRow dr)
        {

            bool ret = false;

            try
            {
                Id = (int)dr.Field<long>("Id");
                NDock = dr.Field<string>("NDock");
                Summ = dr.Field<float>("Summ");
                Status = dr.Field<int>("Status");
                NumbTypeOrder = dr.Field<int>("NumbTypeOrder");
                Comment = dr.Field<string>("Comment");

                byte[] b = dr.Field<byte[]>("ExtData");

                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(b, 0, b.Length);
                    ms.Position = 0;

                    extDataOrder = (iOrder)formatter.Deserialize(ms);
                }

                ret = true;                 
            }
            catch { }
            return ret;
        }

       // Основные поля заказа 
        // Id - Идентификатор; NDock - Номер документа; Summ - Сумма; Status - Статус обработки; Comment - Коментарий
        int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        string nuberDockument;
        public string NDock
        {
            get { return nuberDockument; }
            set { nuberDockument = value; }
        }

        float summ;
        public float Summ
        {
            get { return summ; }
            set { summ = value; }
        }

        int status;
        public int Status
        {
            get { return status; }
            set { status = value; }
        }

        public string StatusStr
        {
            get
            {
                return statusmas[status];
            }
        }

        int numbtypeorder;
        public int NumbTypeOrder
        {
            get { return numbtypeorder; }
            set { numbtypeorder = value; }
        }

        string comment;
        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }


       // Объект расширения конкретного типа заказа
        iOrder extDataOrder = null;
       public iOrder ExtDataOrder
        {
            get { return extDataOrder; }
            set { extDataOrder = value; }
        }

       // Процедура обработки заказа
       public  void Process()
        {
            ExtDataOrder.Process(this);
        }

       // Процедура снятия обработки
        public void UnProcess()
        {
            ExtDataOrder.UnProcess(this);
        }
    }

    // Интерфейс обработчиков заказа
    interface iOrder
    {
        void Process(cOrder order);
        void UnProcess(cOrder order);

    }


    // Класс типа заказа - "Заказ Клиенту"
    [Serializable]
    class cOrderClient : iOrder
    {
        public cOrderClient()
        {
        }

        public cOrderClient(string nm)
        {
            name = nm;
        }

        string name = "";
        [DisplayName("Имя заказа")]
        [ReadOnly(true)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        float percent = -5;
        [DisplayName("Процент")]
        [ReadOnly(true)]  
        public float Percent
        {
            get { return percent; }
            set { percent = value; }
        }

        string fio = "";
        [DisplayName("ФИО")]
        public String FIO
        {
            get { return fio; }
            set { fio = value; }
        }
        string adres = "";
        [DisplayName("Адрес")]
        public String Adres
        {
            get { return adres; }
            set { adres = value; }
        }

        public override string ToString()
        {
            return name;
        }

        // интерфейсные процедуры обработки заказа
        public void Process(cOrder order)
        {
            order.Summ = (order.Summ / 100) * (100 + percent);
            order.Status = 1;
        }
        public void UnProcess(cOrder order)
        {
            order.Summ = (order.Summ / (100 + percent)) * 100;
            order.Status = 0;
        }
    }

    // Класс типа заказа - "Заказ Поставщику"
    [Serializable]
    class cOrderVendor : iOrder
    {
        public cOrderVendor()
        {
        }

        public cOrderVendor(string nm)
        {
            name = nm;
        }

        string name = "";
        [DisplayName("Имя заказа")]
        [ReadOnly(true)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        float percent = 10;
        [DisplayName("Процент")]
        [ReadOnly(true)] 
        public float Percent
        {
            get { return percent; }
            set { percent = value; }
        }

        string inn = "";
        [DisplayName("ИНН")]
        public String INN
        {
            get { return inn; }
            set { inn = value; }
        }

        string fizadres = "";
        [DisplayName("Физический Адрес")]
        public String FizAdres
        {
            get { return fizadres; }
            set { fizadres = value; }
        }

        string jradres = "";
        [DisplayName("Юридический Адрес")]
        public String JrAdres
        {
            get { return jradres; }
            set { jradres = value; }
        }

        public override string ToString()
        {
            return name; 
        }

        // интерфейсные процедуры обработки заказа
        public void Process(cOrder order)
        {
            order.Summ = (order.Summ / 100) * (100 + percent);
            order.Status = 1;
        }
        public void UnProcess(cOrder order)
        {
            order.Summ = (order.Summ / (100 + percent)) * 100;
            order.Status = 0;
        }
    }


    public class TypeCreater
    //Класс для СomboBox для создания объектов на лету
    {
        public string name = "";
        public Type type = null;

        public TypeCreater(string _name, Type _t)
        {
            type = _t;
            name = _name;
        }

        // Процедура создания объкта
        public object CreteClass()
        {

            //System.Reflection.ConstructorInfo ci = type.GetConstructor(new Type[] { });
            //object Obj = ci.Invoke(new object[] { });

            System.Reflection.ConstructorInfo ci = type.GetConstructor(new Type[] { typeof(string) });
            object Obj = ci.Invoke(new object[] {name});

            return Obj;
        }

        public override string ToString()
        {
            return name;
        }

    }


}
