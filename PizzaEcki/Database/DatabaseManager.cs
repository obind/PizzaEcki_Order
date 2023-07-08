using Microsoft.Data.Sqlite;
using System;
using SQLitePCL;
using PizzaEcki.Models;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;

namespace PizzaEcki.Database
{
    class DatabaseManager : IDisposable
    {
        private SqliteConnection _connection;
        private readonly string databaseFilePath = "./database.sqlite";

        //Database Managment
        public DatabaseManager()
        {
            Batteries.Init(); // Initialisierung von SQLitePCLRaw

            _connection = new SqliteConnection($"Data Source={databaseFilePath};");
            string fullPath = System.IO.Path.GetFullPath(databaseFilePath);
            Console.WriteLine(fullPath);

            _connection.Open();

            CreateTable();
            InitializeDishes();


        }
        private void InitializeDishes()
        {
            List<Dish> dishes = new List<Dish>
            {
            //Salate
                new Dish { Id = 10, Name = "Bauernsalat", Price = 8.20 },
                new Dish { Id = 11, Name = "Gemischter Salat", Price = 7.90 },
                new Dish { Id = 12, Name = "Cetrioli Salat", Price = 8.20 },
                new Dish { Id = 13, Name = "Tomaten Salat", Price = 7.90 },
                new Dish { Id = 14, Name = "Mozzarella Salat", Price = 8.20 },
                new Dish { Id = 15, Name = "Thunfisch Salat", Price = 8.90 },
                new Dish { Id = 16, Name = "Italia Salat", Price = 9.20 },
                new Dish { Id = 17, Name = "Fisch Salat", Price = 9.20 },
                new Dish { Id = 18, Name = "Chef Salat", Price = 10.20 },
                new Dish { Id = 19, Name = "Chicken-Salat", Price = 9.90 },
                new Dish { Id = 600, Name = "Lachs-Salat", Price = 9.80 },
                new Dish { Id = 601, Name = "Rucola-Salat", Price = 9.80 },
                new Dish { Id = 602, Name = "Döner-Salat", Price = 9.80 },

            //Baugette
                new Dish { Id = 100, Name = "Käse", Price = 7.30 },
                new Dish { Id = 101, Name = "Mozzarella", Price = 8.60 },
                new Dish { Id = 102, Name = "Salami", Price = 7.80 },
                new Dish { Id = 103, Name = "Schinken", Price = 7.80 },
                new Dish { Id = 104, Name = "Thunfisch", Price = 8.80 },
                new Dish { Id = 105, Name = "Weißkäse", Price = 9.30 },
                new Dish { Id = 106, Name = "Vegetaria", Price = 9.30 },
                new Dish { Id = 107, Name = "Chicken", Price = 10.30 },
                new Dish { Id = 108, Name = "Ei", Price = 7.80 },
                new Dish { Id = 109, Name = "Lachs", Price = 10.30 },
                new Dish { Id = 110, Name = "Hawaii", Price = 8.80 },
                new Dish { Id = 111, Name = "Dönero", Price = 9.30 },
                new Dish { Id = 112, Name = "Knoblauchwurst", Price = 9.30 },
           
            // Pizzen
                new Dish { Id = 20, Name = "Margherita", Price = 6.00 },
                new Dish { Id = 21, Name = "Champignons", Price = 6.90 },
                new Dish { Id = 22, Name = "Spinat", Price = 6.90 },
                new Dish { Id = 23, Name = "Sardellen", Price = 6.90 },
                new Dish { Id = 24, Name = "Cipolla", Price = 6.20 },
                new Dish { Id = 25, Name = "Adrinta", Price = 6.90 },
                new Dish { Id = 26, Name = "Salami", Price = 6.90 },
                new Dish { Id = 27, Name = "Schinken", Price = 6.90 },
                new Dish { Id = 28, Name = "Bolognese", Price = 6.90 },
                new Dish { Id = 29, Name = "Vesuvio", Price = 6.90 },
                new Dish { Id = 30, Name = "Funghi", Price = 6.90 },
                new Dish { Id = 31, Name = "Romana", Price = 7.50 },
                new Dish { Id = 32, Name = "Hawaii", Price = 7.00 },
                new Dish { Id = 33, Name = "Tonno", Price = 7.50 },
                new Dish { Id = 34, Name = "Billa", Price = 7.00 },
                new Dish { Id = 35, Name = "Quattro Stagione", Price = 8.50 },
                new Dish { Id = 36, Name = "Apollo", Price = 7.70 },
                new Dish { Id = 37, Name = "Cosa Nostra", Price = 7.90 },
                new Dish { Id = 38, Name = "Primavera", Price = 7.90 },
                new Dish { Id = 39, Name = "Mista", Price = 7.90 },
                new Dish { Id = 40, Name = "Vegetaria", Price = 7.90 },
                new Dish { Id = 41, Name = "Bacio", Price = 7.90 },
                new Dish { Id = 42, Name = "Calzone", Price = 9.60 },
                new Dish { Id = 43, Name = "Weißkäse", Price = 7.90 },
                new Dish { Id = 44, Name = "Gorgonzola", Price = 7.00 },
                new Dish { Id = 45, Name = "Riesengarnele", Price = 8.80 },
                new Dish { Id = 46, Name = "Laguna", Price = 9.80 },
                new Dish { Id = 47, Name = "Polio", Price = 9.80 },
                new Dish { Id = 48, Name = "Pollana", Price = 9.80 },
                new Dish { Id = 49, Name = "Spezial", Price = 9.80 },
                new Dish { Id = 500, Name = "Capri", Price = 7.00 },
                new Dish { Id = 501, Name = "Spaghetti", Price = 8.50 },
                new Dish { Id = 502, Name = "Mexicana", Price = 8.90 },
                new Dish { Id = 503, Name = "Sicilia", Price = 9.80 },
                new Dish { Id = 504, Name = "Picante", Price = 8.50 },
                new Dish { Id = 505, Name = "Calzone", Price = 9.60 },
                new Dish { Id = 506, Name = "Spargel", Price = 8.80 },
                new Dish { Id = 507, Name = "Döner", Price = 8.90 },
                new Dish { Id = 508, Name = "Döner", Price = 9.50 },
                new Dish { Id = 509, Name = "Calzone", Price = 10.30 },
                new Dish { Id = 510, Name = "Spinat", Price = 7.40 },
                new Dish { Id = 511, Name = "Polio", Price = 9.50 },
                new Dish { Id = 512, Name = "Parmenzola", Price = 8.50 },
                new Dish { Id = 513, Name = "Jalapeno", Price = 9.80 },
                new Dish { Id = 514, Name = "Melanzana", Price = 9.80 },
            };

            AddDishes(dishes);


            List<Extra> extras = new List<Extra>
            {
                new Extra { Id = 1, Name = "Käse", Price = 1.50 },
                new Extra { Id = 2, Name = "Tomatensauce", Price = 1.00 },
                new Extra { Id = 3, Name = "Salami", Price = 1.50 },
                new Extra { Id = 4, Name = "Schinken", Price = 1.50 },
                new Extra { Id = 5, Name = "Pilze", Price = 1.00 },
                new Extra { Id = 6, Name = "Oliven", Price = 1.00 },
                new Extra { Id = 7, Name = "Zwiebeln", Price = 1.00 },
                new Extra { Id = 8, Name = "Paprika", Price = 1.00 },
                new Extra { Id = 9, Name = "Pepperoni", Price = 1.50 },
                new Extra { Id = 10, Name = "Hähnchen", Price = 2.00 },
                new Extra { Id = 11, Name = "Rindfleisch", Price = 2.00 },
                new Extra { Id = 12, Name = "Thunfisch", Price = 2.00 },
                new Extra { Id = 13, Name = "Ananas", Price = 1.00 },
                new Extra { Id = 14, Name = "Spinat", Price = 1.00 },
                new Extra { Id = 15, Name = "Pesto", Price = 1.50 },
                new Extra { Id = 16, Name = "Parmesan", Price = 1.50 },
                new Extra { Id = 17, Name = "Sardellen", Price = 2.00 },
                new Extra { Id = 18, Name = "Knoblauch", Price = 1.00 },
                new Extra { Id = 19, Name = "Rucola", Price = 1.00 },
                new Extra { Id = 20, Name = "Mais", Price = 1.00 }
            };

            AddExtras(extras);
        }

        //Customers
        private void CreateTable()
        {
            //Customer Table
            string sql = "CREATE TABLE IF NOT EXISTS Customers (PhoneNumber TEXT PRIMARY KEY, Name TEXT, Street TEXT, City TEXT, AdditionalInfo TEXT)";

            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }

            //Dishes Table
            sql = "CREATE TABLE IF NOT EXISTS Dishes (Id INTEGER PRIMARY KEY, Name TEXT, Price REAL)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }

            //Extras Table
            sql = "CREATE TABLE IF NOT EXISTS Extras (Id INTEGER PRIMARY KEY, Name TEXT, Price REAL)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }
        }
        public Customer GetCustomerByPhoneNumber(string phoneNumber)
        {
            string sql = "SELECT * FROM Customers WHERE PhoneNumber = @PhoneNumber";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Customer
                        {
                            PhoneNumber = reader.GetString(0),
                            Name = reader.GetString(1),
                            Street = reader.GetString(2),
                            City = reader.GetString(3),
                            AdditionalInfo = reader.IsDBNull(4) ? null : reader.GetString(4)
                        };
                    }
                }
            }

            return null;
        }
        public void AddOrUpdateCustomer(Customer customer)
        {
            string sql = "INSERT OR REPLACE INTO Customers (PhoneNumber, Name, Street, City, AdditionalInfo) VALUES (@PhoneNumber, @Name, @Street, @City, @AdditionalInfo)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
                command.Parameters.AddWithValue("@Name", customer.Name);
                command.Parameters.AddWithValue("@Street", customer.Street);
                command.Parameters.AddWithValue("@City", customer.City);
                command.Parameters.AddWithValue("@AdditionalInfo", (object)customer.AdditionalInfo ?? DBNull.Value);

                command.ExecuteNonQuery();
            }
        }
        
        //Dishes
        public void AddDishes(List<Dish> dishes)
        {
            foreach (var dish in dishes)
            {
                AddOrUpdateDish(dish);
            }
        }
        public void AddOrUpdateDish(Dish dish)
        {
            string sql = "INSERT OR REPLACE INTO Dishes (Id, Name, Price) VALUES (@Id, @Name, @Price)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Id", dish.Id);
                command.Parameters.AddWithValue("@Name", dish.Name);
                command.Parameters.AddWithValue("@Price", dish.Price);

                command.ExecuteNonQuery();
            }
        }
        public List<Dish> GetDishes()
        {
            List<Dish> dishes = new List<Dish>();
            string sql = "SELECT * FROM Dishes";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Dish dish = new Dish
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Price = Convert.ToDouble(reader["Price"])
                        };

                        dishes.Add(dish);
                    }
                }
            }

            return dishes;
        }
        
        //Extras
        public void AddExtras(List<Extra> extras)
        {
            foreach (var extra in extras)
            {
                AddOrUpdateExtra(extra);
            }
        }
        public void AddOrUpdateExtra(Extra extra)
        {
            string sql = "INSERT OR REPLACE INTO Extras (Id, Name, Price) VALUES (@Id, @Name, @Price)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Id", extra.Id);
                command.Parameters.AddWithValue("@Name", extra.Name);
                command.Parameters.AddWithValue("@Price", extra.Price);

                command.ExecuteNonQuery();
            }

        }
        public List<Extra> GetExtras()
        {
            List<Extra> extras = new List<Extra>();
            string sql = "SELECT * FROM Extras";  // Sie müssen die Tabelle Extras erstellen
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        extras.Add(new Extra
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Price = reader.GetDouble(2)
                        });
                    }
                }
            }

            return extras;
        }    
        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
