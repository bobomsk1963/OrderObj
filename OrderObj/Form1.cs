using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

/* Богоявленский Борис Валерьевич - bobomsk1963@mail.ru
 * 
 */


namespace OrderObj
{
    public partial class Form1 : Form
    {
        ClassOpenBase Base;

        // Массив Типов заказа
        TypeCreater[] masTypeOrder = new TypeCreater[] 
        { 
            new TypeCreater("Заказ от Покупателя", typeof(cOrderClient)), 
            new TypeCreater("Заказ Поставщику", typeof(cOrderVendor)) 
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Base = new ClassOpenBase();

            // Инициализация виртуальной таблицы
            listView1.VirtualListSize = Base.classOrder.dataView.Count;
            listView1.VirtualMode = true;
            listView1.Refresh();
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // Событие вывода в виртуальную таблицу Заказов            
            int c = ((ListView)sender).VirtualListSize;
            if (((c > 0) && (e.ItemIndex < c)) && (e.ItemIndex > -1))
            {
                cOrder cO = new cOrder(Base.classOrder.dataView[e.ItemIndex].Row);
                e.Item = new ListViewItem(new string[] { (e.ItemIndex + 1).ToString(), 
                                    cO.Id.ToString(),
                                    cO.NDock,
                                    cO.Summ.ToString(),
                                    cO.StatusStr,
                                    cO.ExtDataOrder.ToString(),
                                    cO.Comment
                                    });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // добавление нового заказа
            FormEditOrder F = new FormEditOrder(-1, Base, masTypeOrder);
            if (F.ShowDialog(this) == DialogResult.OK)
            {
                
                listView1.VirtualListSize = Base.classOrder.dataView.Count;
                listView1.Refresh();
            }

            F.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Изменение заказа
            if (listView1.SelectedIndices.Count > 0)
            {
                int n = listView1.SelectedIndices[0];

                FormEditOrder F = new FormEditOrder(n, Base, masTypeOrder);
                if (F.ShowDialog(this) == DialogResult.OK)
                {

                    listView1.VirtualListSize = Base.classOrder.dataView.Count;
                    listView1.Refresh();
                }
                F.Dispose();
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            // Обработка заказа

            if (listView1.SelectedIndices.Count > 0)
            {
                int n = listView1.SelectedIndices[0];


                DataRowView Drv = Base.classOrder.dataView[n];
                cOrder cO = new cOrder(Base.classOrder.dataView[n].Row);

                if (cO.Status == 0)
                {
                    cO.Process();

                    Drv.BeginEdit();
                    cO.ThisToRow(Drv.Row);
                    Drv.EndEdit();
                    Base.classOrder.UpdateTable();


                    listView1.VirtualListSize = Base.classOrder.dataView.Count;
                    listView1.Refresh();
                }
                else
                {
                    MessageBox.Show(this, "Обработка уже выполнена!", "Внимание!");
                }
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Удаление заказа

            if (listView1.SelectedIndices.Count > 0)
            {
                int n = listView1.SelectedIndices[0];

                if (Base.classOrder.delRow(n))
                {                    
                    listView1.VirtualListSize = Base.classOrder.dataView.Count;
                    listView1.Refresh();
                }
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            // Снятие статус обработки заказа

            if (listView1.SelectedIndices.Count > 0)
            {
                int n = listView1.SelectedIndices[0];

                DataRowView Drv = Base.classOrder.dataView[n];
                cOrder cO = new cOrder(Base.classOrder.dataView[n].Row);

                if (cO.Status == 1)
                {
                    cO.UnProcess();

                    Drv.BeginEdit();
                    cO.ThisToRow(Drv.Row);
                    Drv.EndEdit();
                    Base.classOrder.UpdateTable();


                    listView1.VirtualListSize = Base.classOrder.dataView.Count;
                    listView1.Refresh();
                }
                else
                {
                    MessageBox.Show(this, "Обработка уже выполнена!", "Внимание!");
                }

            }
        }
    }
}
