using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OrderObj
{

    public partial class FormEditOrder : Form
    {

        int Idx = -1;

        ClassOpenBase Base = null;
        cOrder cO = null;
        TypeCreater[] masTypeOrder;


        // Параметры при открытии формы
        // idx - индекс записи в которой будет присходить изменение данных если = -1 то добаление
        // _base - база данных в которой находится изменяемый набор данных
        // mas - массив типов классов вариантов заказов 

        public FormEditOrder(int idx, ClassOpenBase _base, TypeCreater[] mas)
        {
            Base = _base;
            Idx = idx;
            masTypeOrder = mas;
            InitializeComponent();
        }

        private void FormEditOrder_Shown(object sender, EventArgs e)
        {
            // Инициализация формы

            if (Idx < 0)
            {                
                // создание нового заказа
                cO = new cOrder();                
            }
            else
            {
                // Считывание заказа в класс для редактирования
                cO = new cOrder(Base.classOrder.dataView[Idx].Row);
            }


            // инициализация экранных элементов редактирования
            
            
            comboBox1.Items.AddRange(cOrder.statusmas);
            comboBox2.Items.AddRange(masTypeOrder);

            // Связывание переменных класса с экранными формами

            textBox1.DataBindings.Add("Text", cO, "NDock");
            textBox2.DataBindings.Add("Text", cO, "Summ");
            comboBox1.DataBindings.Add("SelectedIndex", cO, "Status");
            comboBox2.DataBindings.Add("SelectedIndex", cO, "NumbTypeOrder");
            textBox3.DataBindings.Add("Text", cO, "Comment");


            // Если статус == 1  тоесть заказ обработан отключаем кнопку для изменения данных
            // заказ можно изменять если отменить обработку
            if (cO.Status == 1)
            {
                button1.Enabled = false;
            }

            // Вывод в PropertyGrid Дополнительных данных заказа
            if (cO.ExtDataOrder != null)
            {
                propertyGrid1.SelectedObject = cO.ExtDataOrder;
            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (cO.ExtDataOrder != null)
            {
                // Сохранение измененных данных
                DataRowView Drv;
                if (Idx < 0)
                {
                    // Если idx<0 то создаем новую запись
                    Drv = Base.classOrder.dataView.AddNew();

                }
                else
                {
                    // определяем запись для редактирования
                    Drv = Base.classOrder.dataView[Idx];
                    Drv.BeginEdit();
                }

                // Производи заполнение измененными данными запись для редактирования
                //Drv.BeginEdit();
                cO.ThisToRow(Drv.Row);
                Drv.EndEdit();
                Base.classOrder.UpdateTable();

            }
            else
            {
                MessageBox.Show(this, "Не выбран тип заказа!", "Внимание!");
            }
        }



        private void comboBox2_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // Событие изменение типа заказа
            if (((ComboBox)sender).SelectedIndex > -1)
            {
                object obj = ((ComboBox)sender).SelectedItem;
                cO.ExtDataOrder = (iOrder)((TypeCreater)obj).CreteClass();

                propertyGrid1.SelectedObject = cO.ExtDataOrder;
            }

        }

        private void FormEditOrder_FormClosed(object sender, FormClosedEventArgs e)
        {
           
        }
    }



}
