using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace PassWordManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SqlConnection sqlConnection = null;
        public MainWindow()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;    //Начальное значение ComboBox
        }


        public static string GetPass(int x, byte sw)    //получение пароля
        {
            string pass = "";
            var rand = new Random();
            switch (sw)     //Определяет, использовать ли спец.символы при генерации пароля
            {
                case 0:     //Спец.символы НЕ используются при генерации пароля
                    while (pass.Length < x)
                    {
                        Char c = (char)rand.Next(33, 125);
                        if (Char.IsLetterOrDigit(c))
                            pass += c;
                    }
                    return pass;
                case 1:     //Спец.символы используются при генерации пароля
                    while (pass.Length < x)
                    {
                        Char c = (char)rand.Next(33, 125);
                        pass += c;
                    }
                    return pass;
                default:
                    return "пусто";
            }
        }

        private void history_SelectionChanged(object sender, SelectionChangedEventArgs e)   //копирование пароля из истории
        {
            string pass_1 = (string)history.Items[history.SelectedIndex];
            Clipboard.SetText(pass_1);
            buff.Content = pass_1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)     //Генерирование пароля
        {
            int x = 8 + comboBox1.SelectedIndex;
            byte y;
            if ((bool)symb_1.IsChecked)
                y = 1;
            else
                y = 0;
            string pass = GetPass(x, y);
            labelPass.Content = pass;
            history.Items.Insert(0, pass);
            Clipboard.SetText(pass);
            buff.Content = pass;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)    //Подключение к БП при загрузке приложения
        {
            //sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PassDB"].ConnectionString);
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Pass"].ConnectionString);
            //sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PassDB1"].ConnectionString);
            sqlConnection.Open();
            if (sqlConnection.State == ConnectionState.Open)   //этот шаг делаем только для проверки подключения
            {
                MessageBox.Show("Подключение установлено");    //Проверяем, есть подключение к БД
            }
            PassWordManager.PassDBDataSet passDBDataSet = ((PassWordManager.PassDBDataSet)(this.FindResource("passDBDataSet")));
            // Загрузить данные в таблицу Table_pass. Можно изменить этот код как требуется.
            PassWordManager.PassDBDataSetTableAdapters.Table_passTableAdapter passDBDataSetTable_passTableAdapter = new PassWordManager.PassDBDataSetTableAdapters.Table_passTableAdapter();
            passDBDataSetTable_passTableAdapter.Fill(passDBDataSet.Table_pass);
            System.Windows.Data.CollectionViewSource table_passViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("table_passViewSource")));
            table_passViewSource.View.MoveCurrentToFirst();
        }

        private void Window_Closed(object sender, EventArgs e)      //Закрытие подключения к БД при выходе из приложения
        {
            sqlConnection.Close();
        }

        private void button_insert_Click(object sender, RoutedEventArgs e)
        {
            bool ser = true, log = true, pas = true;
            if (service.Text == "")
            {
                MessageBox.Show("Введите название сервиса");
                ser = false;
            }
            else if (login.Text == "")
            {
                MessageBox.Show("Введите логин");
                log = false;
            }
            else if (password.Text == "")
            {
                MessageBox.Show("Введите пароль");
                pas = false;
            }

            if (ser && log && pas)
            {
                SqlCommand command = new SqlCommand($"INSERT INTO [Table_pass] (service, login, password, note) VALUES (@service, @login, @password, @note)", sqlConnection);
                command.Parameters.AddWithValue("service", service.Text);
                command.Parameters.AddWithValue("login", login.Text);
                command.Parameters.AddWithValue("password", password.Text);
                command.Parameters.AddWithValue("note", note.Text);
                string result_of_operation = command.ExecuteNonQuery().ToString();
                

                switch (result_of_operation)
                {
                    case "1":
                        result_operation.Content = "Операция произведена успешно";
                        service.Text = "";
                        login.Text = "";
                        password.Text = "";
                        note.Text = "";
                        break;
                    default:
                        result_operation.Content = "Запись не выполнена";
                        break;
                }
            }

        }

        private void service_GotFocus(object sender, RoutedEventArgs e)
        {
            result_operation.Content = "Ожидание данных";
        }
        private void login_GotFocus(object sender, RoutedEventArgs e)
        {
            result_operation.Content = "Ожидание данных";
        }
        private void password_GotFocus(object sender, RoutedEventArgs e)
        {
            result_operation.Content = "Ожидание данных";
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            SqlDataAdapter dataAdapter = new SqlDataAdapter("SELECT * FROM Table_pass", sqlConnection);

            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            
        }
    }
}
