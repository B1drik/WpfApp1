using System;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadAlgorithms();
        }

        private void LoadAlgorithms()
        {
            string connectionString = @"Data Source=dbsrv\dub2024;Initial Catalog=algoritmZolotareva;Integrated Security=True;TrustServerCertificate=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT id, name FROM algorithms";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    AlgorithmComboBox.Items.Add(new Algorithm
                    {
                        Id = (int)reader["id"],
                        Name = (string)reader["name"]
                    });
                }
            }
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlgorithmComboBox.SelectedItem is Algorithm selectedAlgorithm && !string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                string inputText = InputTextBox.Text;
                string resultText = string.Empty;

                switch (selectedAlgorithm.Name)
                {
                    case "AES":
                        resultText = AesEncryption.Encrypt(inputText, "1234567890123456"); // Убедитесь, что длина ключа правильная
                        break;
                    case "Cesar":
                        int shift = 3; // Вы можете изменить значение сдвига по своему усмотрению
                        resultText = CaesarCipher.Encrypt(inputText, shift);
                        break;
                    default:
                        MessageBox.Show("Выберите допустимый алгоритм.");
                        return;
                }

                ResultTextBox.Text = resultText;
                SaveToDatabase(inputText, resultText, selectedAlgorithm.Id); // Сохранение в базу данных
            }
            else
            {
                MessageBox.Show("Введите текст и выберите алгоритм.");
            }
        }

        private void SaveToDatabase(string inputText, string resultText, int algorithmId)
        {
            string connectionString = @"Data Source=dbsrv\dub2024;Initial Catalog=algoritmZolotareva;Integrated Security=True;TrustServerCertificate=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO encryption_results (InputText, ResultText, AlgorithmId) VALUES (@InputText, @ResultText, @AlgorithmId)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InputText", inputText);
                    command.Parameters.AddWithValue("@ResultText", resultText);
                    command.Parameters.AddWithValue("@AlgorithmId", algorithmId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AlgorithmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlgorithmComboBox.SelectedItem != null)
            {
                var selectedAlgorithm = (Algorithm)AlgorithmComboBox.SelectedItem;
                Console.WriteLine($"Выбран алгоритм: {selectedAlgorithm.Name}");
            }
        }

        public class Algorithm
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name; // Отобразим только имя в ComboBox
            }
        }

        public static class AesEncryption
        {
            public static string Encrypt(string text, string key)
            {
                using (Aes aes = Aes.Create())
                {
                    var keyBytes = Encoding.UTF8.GetBytes(key);
                    aes.Key = keyBytes;
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(aes.IV, 0, aes.IV.Length);
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(text);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        public static class CaesarCipher
        {
            public static string Encrypt(string text, int shift)
            {
                StringBuilder result = new StringBuilder();
                foreach (char c in text)
                {
                    if (char.IsLetter(c))
                    {
                        char offset = char.IsUpper(c) ? 'A' : 'a';
                        char encChar = (char)(((c + shift - offset) % 26) + offset);
                        result.Append(encChar);
                    }
                    else
                    {
                        result.Append(c);
                    }
                }

                return result.ToString();
            }
        }
    }
}