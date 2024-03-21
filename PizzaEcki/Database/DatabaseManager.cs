using Microsoft.Data.Sqlite;
using System;
using SQLitePCL;
using PizzaEcki.Models;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.IO;
using SharedLibrary;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.EntityFrameworkCore;

namespace PizzaEcki.Database
{
    public class DatabaseManager : IDisposable
    {
        private SqliteConnection _connection;
        private readonly string userDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private readonly string databaseFolderName = "PizzaEckiDb";
        private readonly string databaseFileName = "database.sqlite";
        private readonly string fullPathToDatabaseFolder;
        private readonly string fullPathToDatabase;

        public DatabaseManager()
        {
            Batteries.Init(); // Initialisierung von SQLitePCLRaw

            fullPathToDatabaseFolder = Path.Combine(userDocumentsFolder, databaseFolderName);
            fullPathToDatabase = Path.Combine(fullPathToDatabaseFolder, databaseFileName);

            // Erstelle den Ordner, falls er nicht existiert
            Directory.CreateDirectory(fullPathToDatabaseFolder);

            _connection = new SqliteConnection($"Data Source={fullPathToDatabase};");
            Console.WriteLine(fullPathToDatabase);

            _connection.Open();

            CreateTable();
            InitializeDishes();
        }
        //Customers
        private void CreateTable()
        {
            _connection.Open();

            //Customer Table
            string sql = "CREATE TABLE IF NOT EXISTS Customers (PhoneNumber TEXT PRIMARY KEY, Name TEXT, AddressId INTEGER, AdditionalInfo TEXT, FOREIGN KEY(AddressId) REFERENCES Addresses(Id))";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }

            ///
            sql = "CREATE TABLE IF NOT EXISTS Addresses (Id INTEGER PRIMARY KEY AUTOINCREMENT, Street TEXT, City TEXT)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }


            //Dishes Table
            sql = "CREATE TABLE IF NOT EXISTS Gerichte (Id INTEGER PRIMARY KEY, Name TEXT, Preis_S REAL, Preis_L REAL, Preis_XL REAL, Kategorie TEXT, HappyHour TEXT, Steuersatz REAL, GratisBeilage INTEGER)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }

            //Extras Table
            sql = "CREATE TABLE IF NOT EXISTS Extras (Id INTEGER PRIMARY KEY, Name TEXT, Preis_S REAL, Preis_L REAL, Preis_XL REAL)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }

            // Driver Table
            sql = "CREATE TABLE IF NOT EXISTS Drivers (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, PhoneNumber TEXT)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }
            InitializeStaticDrivers();


            //Settings
            _connection.Open();
            sql = "CREATE TABLE IF NOT EXISTS Settings (LastResetDate TEXT, CurrentBonNumber INTEGER)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
               
                command.ExecuteNonQuery();
             
            }
 

            // Initialer Eintrag, falls die Tabelle gerade erstellt wurde
            sql = "INSERT INTO Settings (LastResetDate, CurrentBonNumber) SELECT @date, @number WHERE NOT EXISTS (SELECT 1 FROM Settings)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@date", DateTime.Now.Date);
                command.Parameters.AddWithValue("@number", 1);
                command.ExecuteNonQuery();
            }


            //Zuordnungstabelle
            sql = @"
                CREATE TABLE IF NOT EXISTS OrderAssignments (
                    OrderId TEXT,
                    DriverId INTEGER,
                    Price REAL,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,  -- geändert von AssignmentDate zu Timestamp
                    FOREIGN KEY(OrderId) REFERENCES Orders(OrderId),
                    FOREIGN KEY(DriverId) REFERENCES Drivers(Id)
                );
            ";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }


            sql = @"
            CREATE TABLE IF NOT EXISTS Orders (
                OrderId TEXT PRIMARY KEY,
                BonNumber INTEGER,
                IsDelivery BOOLEAN,
                PaymentMethod TEXT,
                CustomerPhoneNumber TEXT,
                Timestamp DATETIME,
                DeliveryUntil TEXT
            )";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }
         
            sql = @"
            CREATE TABLE IF NOT EXISTS OrderItems (
                OrderItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId TEXT,
                Gericht TEXT,
                Größe TEXT,
                Extras TEXT,
                Menge INTEGER,
                Epreis REAL,
                Gesamt REAL,
                Uhrzeit TEXT,
                LieferungsArt INTEGER,
                FOREIGN KEY(OrderId) REFERENCES Orders(OrderId)
            )";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }

            _connection.Close();
        }
        //Tabellen
        public List<string> GetTableNames()
        {
            _connection.Open();
            List<string> tableNames = new List<string>();
            string sql = "SELECT name FROM sqlite_master WHERE type='table';";

            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tableNames.Add(reader.GetString(0));
                    }
                }
            }
            _connection.Close();
            return tableNames;
        }
        public DataTable GetTableData(string tableName)
        {
            _connection.Open();
            DataTable tableData = new DataTable();
            string sql = $"SELECT * FROM {tableName};";

            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    tableData.Load(reader);
                }
            }
            _connection.Close();
            return tableData;
        }
        public Customer GetCustomerByPhoneNumber(string phoneNumber)
        {
            _connection.Open();
            string sql = @"SELECT c.PhoneNumber, c.Name, a.Street, a.City, c.AdditionalInfo 
                   FROM Customers c
                   INNER JOIN Addresses a ON c.AddressId = a.Id
                   WHERE c.PhoneNumber = @PhoneNumber";

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
                            City = reader.IsDBNull(3) ? null : reader.GetString(3), // Überprüfe auf NULL
                            AdditionalInfo = reader.IsDBNull(4) ? null : reader.GetString(4)
                        };
                    }
                }
            }

            _connection.Close();
            return null;
        }
        public async Task<List<Driver>> GetAllDriversAsync()
        {
            _connection.Open();
            List<Driver> drivers = new List<Driver>();

            // SQL-Befehl vorbereiten
            string sql = "SELECT * FROM Drivers";
            using (var command = new SqliteCommand(sql, _connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var driver = new Driver
                        {
                            Id = reader.GetInt32(0), // Annahme: Id ist die erste Spalte
                            Name = reader.GetString(1) // Annahme: Name ist die zweite Spalte
                                                       // Weitere Eigenschaften entsprechend zuweisen
                        };
                        drivers.Add(driver);
                    }
                }
            }
            _connection.Close();
            return drivers;
        }

        public void UpdateCustomerData(Customer customer)
        {
            _connection.Open();
            string sql = @"UPDATE Customers SET Name = @Name, AdditionalInfo = @AdditionalInfo 
                   INNER JOIN Addresses ON Customers.AddressId = Addresses.Id
                   SET Street = @Street, City = @City
                   WHERE PhoneNumber = @PhoneNumber";

            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Name", customer.Name);
                command.Parameters.AddWithValue("@Street", customer.Street);
                command.Parameters.AddWithValue("@City", customer.City);
                command.Parameters.AddWithValue("@AdditionalInfo", customer.AdditionalInfo);
                command.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);

                command.ExecuteNonQuery();
            }

            _connection.Close();
        }

        public void AddOrUpdateCustomer(Customer customer)
        {
            long addressId;

            _connection.Open();
            // Insert or Update Address
            string addressSql = "INSERT OR REPLACE INTO Addresses (Street, City) VALUES (@Street, @City); SELECT last_insert_rowid();";
            using (SqliteCommand addressCommand = new SqliteCommand(addressSql, _connection))
            {
                addressCommand.Parameters.AddWithValue("@Street", customer.Street);
                addressCommand.Parameters.AddWithValue("@City", customer.City);
                addressId = (long)addressCommand.ExecuteScalar();
            }

            // Insert or Update Customer with Address ID
            string sql = "INSERT OR REPLACE INTO Customers (PhoneNumber, Name, AddressId, AdditionalInfo) VALUES (@PhoneNumber, @Name, @AddressId, @AdditionalInfo)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
                command.Parameters.AddWithValue("@Name", customer.Name);
                command.Parameters.AddWithValue("@AddressId", addressId);
                command.Parameters.AddWithValue("@AdditionalInfo", (object)customer.AdditionalInfo ?? DBNull.Value);
                command.ExecuteNonQuery();
            }
            _connection.Close();
        }

        public async Task<List<Order>> GetOrdersWithAssignedDrivers()
        {
            _connection.Open();
            List<Order> assignedOrders = new List<Order>();


            string sql = @"
                       SELECT 
                Orders.OrderId,
                Orders.BonNumber,
                Orders.IsDelivery,
                Orders.PaymentMethod,
                Orders.CustomerPhoneNumber,
                Orders.Timestamp,
                Orders.DeliveryUntil,
                OrderItems.OrderItemId,
                OrderItems.Gericht,
                OrderItems.Extras,
                OrderItems.Größe,
                OrderItems.Menge,
                OrderItems.Epreis,
                OrderItems.Gesamt,
                OrderItems.Uhrzeit,
                OrderItems.LieferungsArt,
                Drivers.Id AS DriverId,
                Drivers.Name AS Name,
                Drivers.PhoneNumber AS DriverPhoneNumber
                FROM 
                Orders
                LEFT JOIN 
                orderAssignments ON Orders.OrderId = orderAssignments.OrderId
                LEFT JOIN 
                Drivers ON orderAssignments.DriverId = Drivers.Id
                LEFT JOIN 
                OrderItems ON Orders.OrderId = OrderItems.OrderId;
                ";

            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var orderIdValue = reader["OrderId"].ToString();
                        if (string.IsNullOrEmpty(orderIdValue))
                        {
                            continue;  // Überspringe diesen Datensatz
                        }
                        Guid currentOrderId = Guid.Parse(orderIdValue);

                        Order order = assignedOrders.FirstOrDefault(o => o.OrderId == currentOrderId);
                        if (order == null)
                        {
                            try
                            {
                                order = new Order
                                {
                                    OrderId = currentOrderId,
                                    BonNumber = reader.IsDBNull(reader.GetOrdinal("BonNumber")) ? 0 : reader.GetInt32(reader.GetOrdinal("BonNumber")),
                                    IsDelivery = reader.IsDBNull(reader.GetOrdinal("IsDelivery")) ? false : reader.GetBoolean(reader.GetOrdinal("IsDelivery")),
                                    PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                    CustomerPhoneNumber = reader.IsDBNull(reader.GetOrdinal("CustomerPhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("CustomerPhoneNumber")),
                                    Timestamp = reader.IsDBNull(reader.GetOrdinal("Timestamp")) ? null : reader.GetDateTime(reader.GetOrdinal("Timestamp")).ToString(),
                                    DeliveryUntil = reader.IsDBNull(reader.GetOrdinal("DeliveryUntil")) ? null : reader.GetString(reader.GetOrdinal("DeliveryUntil")),
                                    DriverId = reader.IsDBNull(reader.GetOrdinal("DriverId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("DriverId")),
                                    Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                                    OrderItems = new List<OrderItem>()
                                };
                            }
                            catch (Exception ex)
                            {
                                // Logge den Fehler, z.B. durch Ausgabe auf der Konsole oder in einer Datei
                                Console.WriteLine("Fehler beim Erstellen des Order-Objekts: " + ex.Message);
                                throw; // Wirf den Fehler weiter nach oben, damit du weißt, dass etwas schiefgelaufen ist.
                            }
                            assignedOrders.Add(order);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("OrderItemId")))
                        {
                            OrderItem orderItem = new OrderItem
                            {
                                Nr = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                                Gericht = reader.IsDBNull(reader.GetOrdinal("Gericht")) ? null : reader.GetString(reader.GetOrdinal("Gericht")),
                                Extras = reader.IsDBNull(reader.GetOrdinal("Extras")) ? null : reader.GetString(reader.GetOrdinal("Extras")),
                                Größe = reader.IsDBNull(reader.GetOrdinal("Größe")) ? null : reader.GetString(reader.GetOrdinal("Größe")),
                                Menge = reader.IsDBNull(reader.GetOrdinal("Menge")) ? 0 : reader.GetInt32(reader.GetOrdinal("Menge")),
                                Epreis = reader.IsDBNull(reader.GetOrdinal("Epreis")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Epreis")),
                                Gesamt = reader.IsDBNull(reader.GetOrdinal("Gesamt")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Gesamt")),
                                Uhrzeit = reader.IsDBNull(reader.GetOrdinal("Uhrzeit")) ? null : reader.GetString(reader.GetOrdinal("Uhrzeit")),
                                LieferungsArt = reader.IsDBNull(reader.GetOrdinal("LieferungsArt")) ? 0 : reader.GetInt32(reader.GetOrdinal("LieferungsArt"))
                            };
                            order.OrderItems.Add(orderItem);
                        }
                    }
                }
            }

            await _connection.CloseAsync();

            return assignedOrders;
        }

        //Dishes
        public void AddDishes(List<Dish> dishes)
        {
            _connection.Open();
            foreach (var dish in dishes)
            {
                AddOrUpdateDish(dish);
            }
            _connection.Close();
        }
        public void AddOrUpdateDish(Dish dish)
        {
            _connection.Open();
            string sql = "INSERT OR REPLACE INTO Gerichte (Id, Name, Preis_S, Preis_L, Preis_XL, Kategorie, HappyHour, Steuersatz, GratisBeilage) VALUES (@Id, @Name, @Preis_S, @Preis_L, @Preis_XL, @Kategorie, @HappyHour, @Steuersatz, @GratisBeilage)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Id", dish.Id);
                command.Parameters.AddWithValue("@Name", dish.Name);
                command.Parameters.AddWithValue("@Preis_S", dish.Preis_S);
                command.Parameters.AddWithValue("@Preis_L", dish.Preis_L);
                command.Parameters.AddWithValue("@Preis_XL", dish.Preis_XL);
                command.Parameters.AddWithValue("@Kategorie", dish.Kategorie.ToString());
                command.Parameters.AddWithValue("@HappyHour", dish.HappyHour);
                command.Parameters.AddWithValue("@Steuersatz", dish.Steuersatz);
                command.Parameters.AddWithValue("@GratisBeilage", dish.GratisBeilage);
                command.ExecuteNonQuery();
            }
            _connection.Close();
        }
        public List<Dish> GetAllDishes()
        {
            _connection.Open();
            List<Dish> dishes = new List<Dish>();
            string sql = "SELECT * FROM Gerichte";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Dish dish = new Dish();

                        if (int.TryParse(reader["Id"].ToString(), out int Id))
                        {
                            dish.Id = Id;
                        }

                        dish.Name = reader["Name"].ToString();

                        if (double.TryParse(reader["Preis_S"].ToString(), out double preis_s))
                        {
                            dish.Preis_S = preis_s;
                        }

                        if (double.TryParse(reader["Preis_L"].ToString(), out double preis_l))
                        {
                            dish.Preis_L = preis_l;
                        }

                        if (double.TryParse(reader["Preis_XL"].ToString(), out double preis_xl))
                        {
                            dish.Preis_XL = preis_xl;
                        }


                         dish.Kategorie = Enum.Parse<DishCategory>(reader["Kategorie"].ToString());
                       

                        dish.HappyHour = reader["HappyHour"].ToString();

                        if (double.TryParse(reader["Steuersatz"].ToString(), out double steuersatz))
                        {
                            dish.Steuersatz = steuersatz;
                        }

                        if (int.TryParse(reader["GratisBeilage"].ToString(), out int gratisBeilage))
                        {
                            dish.GratisBeilage = gratisBeilage;
                        }

                        dishes.Add(dish);
                    }
                }
            }
            _connection.Close();
            return dishes;
        }
        public bool IsIdExists(int Id)
        {
            _connection.Open();
            string sql = "SELECT COUNT(*) FROM Gerichte WHERE Id = @Id";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Id", Id);
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result) > 0;
            }

            _connection.Close();
        }
        public void DeleteDish(int id)
        {
            _connection.Open();
            string sql = "DELETE FROM Gerichte WHERE Id = @Id";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
            _connection.Close();
        }
        //public List<string> GetAllStreets()
        //{

        //    List<string> streets = new List<string>();
        //    string sql = "SELECT DISTINCT Street FROM Addresses";
        //    using (SqliteCommand command = new SqliteCommand(sql, _connection))
        //    {
        //        using (SqliteDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                streets.Add(reader.GetString(0));
        //            }
        //        }
        //    }
        //    return streets;
        //}
        public List<string> GetAllCities()
        {
            _connection.Open();
            List<string> cities = new List<string>();
            string sql = "SELECT DISTINCT City FROM Addresses";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cities.Add(reader.GetString(0));
                    }
                }
            }
            _connection.Close();
            return cities;
        }
        //public List<Dish> GetDishes()
        //{
        //    List<Dish> dishes = new List<Dish>();
        //    string sql = "SELECT * FROM Gerichte";
        //    using (SqliteCommand command = new SqliteCommand(sql, _connection))
        //    {
        //        using (SqliteDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                Dish dish = new Dish
        //                {
        //                    Id = Convert.ToInt32(reader["Id"]),
        //                    Name = reader["Name"].ToString(),
        //                    Preis = Convert.ToDouble(reader["Preis"]),
        //                    Kategorie = (DishCategory)Convert.ToInt32(reader["Kategorie"]),
        //                    Größe = reader["Größe"].ToString()
        //                };

        //                dishes.Add(dish);
        //            }
        //        }
        //    }
        //    return dishes;
        //}


        //Extras
        public void AddExtras(List<Extra> extras)
        {
            _connection.Open();
            foreach (var extra in extras)
            {
                AddOrUpdateExtra(extra);
            }
            _connection.Close();
        }

        public void AddOrUpdateExtra(Extra extra)
        {
            _connection.Open();
            string sql = "INSERT OR REPLACE INTO Extras (Id, Name, Preis_S, Preis_L, Preis_XL) VALUES (@Id, @Name, @Preis_S, @Preis_L, @Preis_XL)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Id", extra.Id);
                command.Parameters.AddWithValue("@Name", extra.Name);
                command.Parameters.AddWithValue("@Preis_S", extra.ExtraPreis_S);
                command.Parameters.AddWithValue("@Preis_L", extra.ExtraPreis_L);
                command.Parameters.AddWithValue("@Preis_XL", extra.ExtraPreis_XL);

                command.ExecuteNonQuery();
            }
            _connection.Close();
        }

        public List<Extra> GetExtras()
        {
            _connection.Open();
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
                            ExtraPreis_S = reader.GetDouble(2),
                            ExtraPreis_L = reader.GetDouble(3), 
                             ExtraPreis_XL = reader.GetDouble(4)

                        });
                    }
                }
            }
             _connection.Close();
            return extras;
        }

        //Driver Methodes
        public void AddDriver(Driver driver)
        {
            _connection.Open();
            string sql = "INSERT INTO Drivers (Name, PhoneNumber) VALUES (@Name, @PhoneNumber)";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Name", driver.Name);
                command.Parameters.AddWithValue("@PhoneNumber", driver.PhoneNumber);
                command.ExecuteNonQuery();
            }
            _connection.Close();
        }
        public void UpdateDriver(Driver driver)
        {
            _connection.Open();
            string sql = "UPDATE Drivers SET Name = @Name, PhoneNumber = @PhoneNumber WHERE Id = @Id";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Id", driver.Id);
                command.Parameters.AddWithValue("@Name", driver.Name);
                command.Parameters.AddWithValue("@PhoneNumber", driver.PhoneNumber);
                command.ExecuteNonQuery();
            }
            _connection.Close();
        }
        public void InitializeStaticDrivers()
        {
            _connection.Open();
            string sql = "INSERT OR IGNORE INTO Drivers (Id, Name, PhoneNumber) VALUES (-1, 'Theke', ''), (-2, 'Kasse1', '')";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }
            _connection.Close();
        }
        public void DeleteDriver(int id)
        {
            _connection.Open();
            string sql = "DELETE FROM Drivers WHERE Id = @Id";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
            _connection.Close();
        }
        public void UnassignDriverFromOrders(int driverId)
        {
            string updateSql = @"
                UPDATE OrderAssignments
                SET DriverId = -500
                WHERE DriverId = @DriverId;";

            _connection.Open();

            using (var command = new SqliteCommand(updateSql, _connection))
            {
                command.Parameters.AddWithValue("@DriverId", driverId);
                command.ExecuteNonQuery();
            }
            _connection.Close();
        }

        public List<Driver> GetDrivers()
        {
            _connection.Open();
            List<Driver> drivers = new List<Driver>();
            string sql = "SELECT Id, Name, PhoneNumber FROM Drivers";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    drivers.Add(new Driver
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        PhoneNumber = reader.GetString(2),
                    });
                }
            }
            return drivers;
            _connection.Close();
        }

        public List<Driver> GetAllDrivers()
        {
            _connection.Open();
            List<Driver> drivers = new List<Driver>();

            // SQL-Abfrage, um alle Fahrer abzurufen
            string sql = "SELECT * FROM Drivers";

            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Driver driver = new Driver
                        {
                            // Stellen Sie sicher, dass Sie die Spalten in der richtigen Reihenfolge abrufen
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            PhoneNumber = reader.GetString(2)
                        };
                        drivers.Add(driver);
                    }
                }
            }
            _connection.Close();
            return drivers;
        }

        public int GetCurrentBonNumber()
        {
            string sql = "SELECT LastResetDate, CurrentBonNumber FROM Settings";
            _connection.Open();
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return int.Parse(reader["CurrentBonNumber"].ToString());
                    }
                    else
                    {
                        // Fehlerbehandlung, falls kein Eintrag gefunden wurde
                        throw new Exception("Settings entry not found in the database.");
                    }
                }
            }
            _connection.Close();    
        }

        public async Task<bool> DeleteOrderAsync(Guid orderId)
        {
            
            try
            {
                await _connection.OpenAsync();

                using (var transaction = _connection.BeginTransaction())
                {
                    int result = 0;

                    // Lösche zugehörige Einträge in OrderItems
                    string sqlDeleteOrderItems = "DELETE FROM OrderItems WHERE OrderId = @OrderId;";
                    using (var command = new SqliteCommand(sqlDeleteOrderItems, _connection, transaction))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId.ToString());
                        result += await command.ExecuteNonQueryAsync();
                    }

                    // Lösche zugehörige Einträge in OrderAssignments
                    string sqlDeleteAssignments = "DELETE FROM OrderAssignments WHERE OrderId = @OrderId;";
                    using (var command = new SqliteCommand(sqlDeleteAssignments, _connection, transaction))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId.ToString());
                        result += await command.ExecuteNonQueryAsync();
                    }

                    // Lösche den Eintrag in der Orders-Tabelle
                    string sqlDeleteOrder = "DELETE FROM Orders WHERE OrderId = @OrderId;";
                    using (var command = new SqliteCommand(sqlDeleteOrder, _connection, transaction))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId.ToString());
                        result += await command.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();

                    _connection.Close();
                    return result > 0;
                }
               
            }
            catch (Exception ex)
            {
                // Loggen Sie die Ausnahme oder handeln Sie sie entsprechend.
                // Zum Beispiel:
                // _logger.LogError(ex, "An error occurred while deleting order with ID {OrderId}", orderId);
                return false;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public void SaveOrderAssignment(string orderId, int driverId, double price)
        {
            _connection.Open();
            string checkSql = "SELECT COUNT(*) FROM OrderAssignments WHERE OrderId = @OrderId";
            using (SqliteCommand checkCommand = new SqliteCommand(checkSql, _connection))
            {
                checkCommand.Parameters.AddWithValue("@OrderId", orderId);
                int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                if (count > 0)
                {
                    // Ein Eintrag für die angegebene OrderId existiert bereits, aktualisieren Sie ihn
                    string updateSql = "UPDATE OrderAssignments SET DriverId = @DriverId, Price = @Price, Timestamp = @Timestamp WHERE OrderId = @OrderId";
                    using (SqliteCommand updateCommand = new SqliteCommand(updateSql, _connection))
                    {
                        updateCommand.Parameters.AddWithValue("@OrderId", orderId);
                        updateCommand.Parameters.AddWithValue("@DriverId", driverId);
                        updateCommand.Parameters.AddWithValue("@Price", price);
                        updateCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        updateCommand.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Kein Eintrag für die angegebene OrderId, erstellen Sie einen neuen Eintrag
                    string insertSql = "INSERT INTO OrderAssignments (OrderId, DriverId, Price, Timestamp) VALUES (@OrderId, @DriverId, @Price, @Timestamp)";
                    using (SqliteCommand insertCommand = new SqliteCommand(insertSql, _connection))
                    {
                        insertCommand.Parameters.AddWithValue("@OrderId", orderId);
                        insertCommand.Parameters.AddWithValue("@DriverId", driverId);
                        insertCommand.Parameters.AddWithValue("@Price", price);
                        insertCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
            _connection.Close();
        }

        public void SaveOrder(Order order)
        {
            _connection.Open();
            // Speichern der Bestellung in der Orders Tabelle
            string sqlOrder = "INSERT INTO Orders (OrderId, BonNumber,IsDelivery,PaymentMethod,CustomerPhoneNumber, Timestamp, DeliveryUntil) VALUES (@OrderId, @BonNumber, @IsDelivery, @PaymentMethod, @CustomerPhoneNumber, @Timestamp, @DeliveryUntil)";
            using (SqliteCommand commandOrder = new SqliteCommand(sqlOrder, _connection))
            {
                commandOrder.Parameters.AddWithValue("@OrderId", order.OrderId.ToString());
                commandOrder.Parameters.AddWithValue("@BonNumber", order.BonNumber);
                commandOrder.Parameters.AddWithValue("@IsDelivery", order.IsDelivery);
                commandOrder.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod);
                commandOrder.Parameters.AddWithValue("@CustomerPhoneNumber", order.CustomerPhoneNumber);
                commandOrder.Parameters.AddWithValue("@Timestamp", order.Timestamp);
                commandOrder.Parameters.AddWithValue("@DeliveryUntil", order.DeliveryUntil);
                commandOrder.ExecuteNonQuery();
            }

            // Speichern der OrderItems in der OrderItems Tabelle
            foreach (var item in order.OrderItems)
            {
                string sqlItem = @"
                    INSERT INTO OrderItems 
                    (
                        OrderId, 
                        Gericht, 
                        Größe,
                        Extras, 
                        Menge, 
                        Epreis, 
                        Gesamt, 
                        Uhrzeit, 
                        LieferungsArt
                    ) 
                    VALUES 
                    (
                        @OrderId, 
                        @Gericht, 
                        @Größe,
                        @Extras, 
                        @Menge, 
                        @Epreis, 
                        @Gesamt, 
                        @Uhrzeit, 
                        @LieferungsArt
                    )
                ";
                using (SqliteCommand commandItem = new SqliteCommand(sqlItem, _connection))
                {
                    commandItem.Parameters.AddWithValue("@OrderId", order.OrderId.ToString());
                    commandItem.Parameters.Add("@Gericht", SqliteType.Text).Value = (object)item.Gericht ?? DBNull.Value;
                    commandItem.Parameters.Add("@Größe", SqliteType.Text).Value = (object)item.Größe ?? DBNull.Value;
                    commandItem.Parameters.Add("@Extras", SqliteType.Text).Value = (object)item.Extras ?? DBNull.Value;
                    commandItem.Parameters.Add("@Menge", SqliteType.Integer).Value = (object)item.Menge ?? DBNull.Value;
                    commandItem.Parameters.Add("@Epreis", SqliteType.Real).Value = (object)item.Epreis ?? DBNull.Value;
                    commandItem.Parameters.Add("@Gesamt", SqliteType.Real).Value = (object)item.Gesamt ?? DBNull.Value;
                    commandItem.Parameters.Add("@Uhrzeit", SqliteType.Text).Value = (object)item.Uhrzeit ?? DBNull.Value;
                    commandItem.Parameters.Add("@LieferungsArt", SqliteType.Integer).Value = (object)item.LieferungsArt ?? DBNull.Value;

                    commandItem.ExecuteNonQuery();
                }
            }

            // Erstellen eines Eintrags in der OrderAssignments Tabelle mit einer NULL DriverId
            // Erstellen eines Eintrags in der OrderAssignments Tabelle mit einer NULL DriverId
            string sqlAssignment = "INSERT INTO OrderAssignments (OrderId, DriverId) VALUES (@OrderId, NULL)";
            using (SqliteCommand commandAssignment = new SqliteCommand(sqlAssignment, _connection))
            {
                commandAssignment.Parameters.AddWithValue("@OrderId", order.OrderId.ToString());
                commandAssignment.ExecuteNonQuery();
            }
            _connection.Close();
        }

        public List<Order> GetUnassignedOrders()
        {
            _connection.Open();
            List<Order> unassignedOrders = new List<Order>();
            string sql = @"
                SELECT 
                    Orders.*,
                    OrderItems.*
                FROM 
                    Orders
                LEFT JOIN 
                    OrderItems ON Orders.OrderId = OrderItems.OrderId
                LEFT JOIN 
                    OrderAssignments ON Orders.OrderId = OrderAssignments.OrderId
                WHERE 
                    OrderAssignments.DriverId IS NULL
            ";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var orderIdValue = reader["OrderId"].ToString();
                        if (string.IsNullOrEmpty(orderIdValue))
                        {
                            var bonNumber = reader["BonNumber"].ToString();
                            Console.WriteLine($"Fehler: OrderId ist null oder leer für BonNumber: {bonNumber}");
                            continue;  // Überspringe diesen Datensatz
                        }
                        Guid currentOrderId = Guid.Parse(orderIdValue);

                        Order order;
                        if (unassignedOrders.Any(o => o.OrderId == currentOrderId))
                        {
                            order = unassignedOrders.First(o => o.OrderId == currentOrderId);
                        }
                        else
                        {
                            // Hier nimmst du die Daten für IsDelivery aus der Datenbank
                            var isDeliveryValue = reader["IsDelivery"];
                            bool isDelivery = false;

                            // Wenn der Wert aus der Datenbank kommt, musst du ihn entsprechend konvertieren.
                            if (isDeliveryValue != DBNull.Value)
                            {
                                isDelivery = Convert.ToInt32(isDeliveryValue) != 0;
                            }

                            order = new Order
                            {
                                OrderId = currentOrderId,
                                CustomerPhoneNumber = reader["CustomerPhoneNumber"].ToString(),
                                // Konvertiere DateTime? zu String, wenn es nicht null ist, sonst setze String auf null
                                Timestamp = reader.IsDBNull(reader.GetOrdinal("Timestamp")) ? null : reader.GetDateTime(reader.GetOrdinal("Timestamp")).ToString("o"),                                                                                                                                           
                                DeliveryUntil = reader.IsDBNull(reader.GetOrdinal("DeliveryUntil")) ? null : reader["DeliveryUntil"].ToString(),
                                PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader["PaymentMethod"].ToString(),
                                BonNumber = Convert.ToInt32(reader["BonNumber"]),
                                IsDelivery = isDelivery,
                            };
                            unassignedOrders.Add(order);

                        }

                        OrderItem orderItem = new OrderItem
                        {
                            Nr = reader.IsDBNull(reader.GetOrdinal("OrderItemId")) ? 0 : reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                            Gericht = reader.IsDBNull(reader.GetOrdinal("Gericht")) ? null : reader.GetString(reader.GetOrdinal("Gericht")),
                            Extras = reader.IsDBNull(reader.GetOrdinal("Extras")) ? null : reader.GetString(reader.GetOrdinal("Extras")),
                            Größe = reader.IsDBNull(reader.GetOrdinal("Größe")) ? null : reader.GetString(reader.GetOrdinal("Größe")),
                            Menge = reader.IsDBNull(reader.GetOrdinal("Menge")) ? 0 : reader.GetInt32(reader.GetOrdinal("Menge")),
                            Epreis = reader.IsDBNull(reader.GetOrdinal("Epreis")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Epreis")),
                            Gesamt = reader.IsDBNull(reader.GetOrdinal("Gesamt")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Gesamt")),
                            Uhrzeit = reader.IsDBNull(reader.GetOrdinal("Uhrzeit")) ? null : reader.GetString(reader.GetOrdinal("Uhrzeit")),
                            LieferungsArt = reader.IsDBNull(reader.GetOrdinal("LieferungsArt")) ? 0 : reader.GetInt32(reader.GetOrdinal("LieferungsArt"))
                        };

                        order.OrderItems.Add(orderItem);
                    }
                }
            }
            _connection.Close();
            return unassignedOrders;
        }

        public List<Order> GetAllOrders()
        {
            _connection.Open();
            List<Order> orders = new List<Order>();
            string sql = @"
                SELECT 
                    Orders.*,d
                    OrderItems.*
                FROM 
                    Orders
                LEFT JOIN 
                    OrderItems ON Orders.OrderId = OrderItems.OrderId
                LEFT JOIN 
                    OrderAssignments ON Orders.OrderId = OrderAssignments.OrderId
            ";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var orderIdValue = reader["OrderId"].ToString();
                        if (string.IsNullOrEmpty(orderIdValue))
                        {
                            var bonNumber = reader["BonNumber"].ToString();
                            Console.WriteLine($"Fehler: OrderId ist null oder leer für BonNumber: {bonNumber}");
                            continue;  // Überspringe diesen Datensatz
                        }
                        Guid currentOrderId = Guid.Parse(orderIdValue);

                        Order order;
                        if (orders.Any(o => o.OrderId == currentOrderId))
                        {
                            order = orders.First(o => o.OrderId == currentOrderId);
                        }
                        else
                        {
                            // Hier nimmst du die Daten für IsDelivery aus der Datenbank
                            var isDeliveryValue = reader["IsDelivery"];
                            bool isDelivery = false;

                            // Wenn der Wert aus der Datenbank kommt, musst du ihn entsprechend konvertieren.
                            if (isDeliveryValue != DBNull.Value)
                            {
                                isDelivery = Convert.ToInt32(isDeliveryValue) != 0;
                            }

                            order = new Order
                            {
                                OrderId = currentOrderId,
                                CustomerPhoneNumber = reader["CustomerPhoneNumber"].ToString(),
                                // Konvertiere DateTime? zu String, wenn es nicht null ist, sonst setze String auf null
                                Timestamp = reader.IsDBNull(reader.GetOrdinal("Timestamp")) ? null : reader.GetDateTime(reader.GetOrdinal("Timestamp")).ToString("o"),
                                DeliveryUntil = reader.IsDBNull(reader.GetOrdinal("DeliveryUntil")) ? null : reader["DeliveryUntil"].ToString(),
                                PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader["PaymentMethod"].ToString(),
                                BonNumber = Convert.ToInt32(reader["BonNumber"]),
                                IsDelivery = isDelivery,
                            };
                            orders.Add(order);

                        }

                        OrderItem orderItem = new OrderItem
                        {
                            Nr = reader.IsDBNull(reader.GetOrdinal("OrderItemId")) ? 0 : reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                            Gericht = reader.IsDBNull(reader.GetOrdinal("Gericht")) ? null : reader.GetString(reader.GetOrdinal("Gericht")),
                            Extras = reader.IsDBNull(reader.GetOrdinal("Extras")) ? null : reader.GetString(reader.GetOrdinal("Extras")),
                            Größe = reader.IsDBNull(reader.GetOrdinal("Größe")) ? null : reader.GetString(reader.GetOrdinal("Größe")),
                            Menge = reader.IsDBNull(reader.GetOrdinal("Menge")) ? 0 : reader.GetInt32(reader.GetOrdinal("Menge")),
                            Epreis = reader.IsDBNull(reader.GetOrdinal("Epreis")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Epreis")),
                            Gesamt = reader.IsDBNull(reader.GetOrdinal("Gesamt")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Gesamt")),
                            Uhrzeit = reader.IsDBNull(reader.GetOrdinal("Uhrzeit")) ? null : reader.GetString(reader.GetOrdinal("Uhrzeit")),
                            LieferungsArt = reader.IsDBNull(reader.GetOrdinal("LieferungsArt")) ? 0 : reader.GetInt32(reader.GetOrdinal("LieferungsArt"))
                        };

                        order.OrderItems.Add(orderItem);

                    }
                }
            }
            _connection.Close();    
            return orders;
        }


        public async Task DeleteDailyOrdersAsync()
        {
            // Öffnen Sie die Verbindung zur Datenbank
            _connection.Open();

            // Beginnen Sie eine Transaktion
            using (var transaction = _connection.BeginTransaction())
            {
                // Erstellen und Ausführen des SQL-Befehls zum Löschen von OrderAssignments
                string sqlDeleteOrderAssignments = "DELETE FROM OrderAssignments";
                using (SqliteCommand commandDeleteOrderAssignments = new SqliteCommand(sqlDeleteOrderAssignments, _connection))
                {
                    commandDeleteOrderAssignments.Transaction = transaction; // Verknüpfung mit der Transaktion
                    commandDeleteOrderAssignments.ExecuteNonQuery();
                }

                // Erstellen und Ausführen des SQL-Befehls zum Löschen von OrderItems
                string sqlDeleteOrderItems = "DELETE FROM OrderItems";
                using (SqliteCommand commandDeleteOrderItems = new SqliteCommand(sqlDeleteOrderItems, _connection))
                {
                    commandDeleteOrderItems.Transaction = transaction; // Verknüpfung mit der Transaktion
                    commandDeleteOrderItems.ExecuteNonQuery();
                }

                // Erstellen und Ausführen des SQL-Befehls zum Löschen von Orders
                string sqlDeleteOrders = "DELETE FROM Orders";
                using (SqliteCommand commandDeleteOrders = new SqliteCommand(sqlDeleteOrders, _connection))
                {
                    commandDeleteOrders.Transaction = transaction; // Verknüpfung mit der Transaktion
                    commandDeleteOrders.ExecuteNonQuery();
                }

                // Bestätigen Sie die Transaktion
                transaction.Commit();
            }

            // Schließen Sie die Verbindung zur Datenbank
            _connection.Close();
        }


        public List<OrderItem> GetOrderItems(Guid orderId)
        {
            _connection.Open();
            List<OrderItem> orderItems = new List<OrderItem>();
            string sql = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@OrderId", orderId);
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        OrderItem orderItem = new OrderItem
                        {
                            Nr = reader.GetInt32(reader.GetOrdinal("Nr")),
                            Gericht = reader.GetString(reader.GetOrdinal("Gericht")),
                            Größe = reader.GetString(reader.GetOrdinal("Größe")),
                            Extras = reader.GetString(reader.GetOrdinal("Extras")),
                            Menge = reader.GetInt32(reader.GetOrdinal("Menge")),
                            Epreis = reader.GetDouble(reader.GetOrdinal("Epreis")),
                            Gesamt = reader.GetDouble(reader.GetOrdinal("Gesamt")),
                            Uhrzeit = reader.GetString(reader.GetOrdinal("Uhrzeit")),
                            LieferungsArt = reader.GetInt32(reader.GetOrdinal("LieferungsArt"))
                        };
                        orderItems.Add(orderItem);
                    }
                }
            }
            _connection.Close();
            return orderItems;
        }

        public List<OrderAssignment> GetOrderAssignments()
        {
            _connection.Open();
            List<OrderAssignment> assignments = new List<OrderAssignment>();
            string sql = "SELECT OrderId, DriverId, Price, Timestamp FROM OrderAssignments";  // Preis hinzugefügt
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0) || reader.IsDBNull(1) || reader.IsDBNull(2) || reader.IsDBNull(3))
                        {
                            // Wenn irgendein Feld null ist, überspringe diesen Datensatz.
                            continue;
                        }

                        OrderAssignment assignment = new OrderAssignment
                        {
                            BonNumber = reader.GetInt32(0),
                            DriverId = reader.GetInt32(1),
                            Price = reader.GetDouble(2),
                            Timestamp = reader.GetDateTime(3)
                        };
                        assignments.Add(assignment);
                    }
                }
            }
            _connection.Close();
            return assignments;
            
        }

        public double GetTotalSalesForDate(DateTime date)
        {
            _connection.Open();
            string sql = "SELECT SUM(Price) FROM OrderAssignments WHERE AssignmentDate = @Date";
            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                object result = command.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToDouble(result) : 0;
               
            }
            
        }

        public List<DailySalesInfo> GetDailySales(DateTime date)
        {
            _connection.Open();
            List<DailySalesInfo> dailySalesInfoList = new List<DailySalesInfo>();

            // SQL-Query, um die täglichen Umsätze abzurufen
            string sql = @"
            SELECT
                IFNULL(Drivers.Name, 'Theke') as Name,
                SUM(OrderAssignments.Price) as DailySales
            FROM
                OrderAssignments
            LEFT JOIN
                Drivers ON OrderAssignments.DriverId = Drivers.Id
            WHERE
                date(OrderAssignments.Timestamp) = date(@Date)
            GROUP BY
                IFNULL(Drivers.Name, 'Theke');
            ";

            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DailySalesInfo dailySalesInfo = new DailySalesInfo
                        {
                            Name = reader.GetString(0),
                            DailySales = reader.IsDBNull(1) ? 0 : reader.GetDouble(1)  // Überprüfung auf NULL
                        };
                        dailySalesInfoList.Add(dailySalesInfo);
                    }
                }
            }
            _connection.Close();
            return dailySalesInfoList;
        }
        public int CheckAndResetBonNumberIfNecessary()
        {
            _connection.Open();
            string getSettingsSql = "SELECT LastResetDate, CurrentBonNumber FROM Settings LIMIT 1";
            string updateSettingsSql = "UPDATE Settings SET CurrentBonNumber = 1, LastResetDate = @LastResetDate";
            int currentBonNumber = 0;

            using (SqliteCommand getSettingsCommand = new SqliteCommand(getSettingsSql, _connection))
            {
                using (SqliteDataReader reader = getSettingsCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var lastResetDateValue = reader["LastResetDate"].ToString();
                        DateTime lastResetDate;

                        // Prüfe, ob der Wert nicht leer ist, und versuche ihn zu parsen
                        if (!string.IsNullOrEmpty(lastResetDateValue) && DateTime.TryParse(lastResetDateValue, out lastResetDate))
                        {
                            currentBonNumber = int.Parse(reader["CurrentBonNumber"].ToString());
                            var currentDate = DateTime.Now.Date;

                            if (currentDate > lastResetDate)
                            {
                                // Zurücksetzen der Bonnummer und Aktualisieren des Reset-Datums
                                using (SqliteCommand updateSettingsCommand = new SqliteCommand(updateSettingsSql, _connection))
                                {
                                    updateSettingsCommand.Parameters.AddWithValue("@LastResetDate", currentDate.ToString("yyyy-MM-dd"));
                                    updateSettingsCommand.ExecuteNonQuery();
                                }

                                // Setze die Bonnummer auf den Anfangswert zurück
                                currentBonNumber = 0;
                            }
                        }
                        else
                        {
                            // Der Wert ist leer oder null, setze LastResetDate auf das heutige Datum
                            lastResetDate = DateTime.Now.Date;

                            // Erstelle eine SQL-Anweisung, um das LastResetDate auf das heutige Datum zu setzen
                            string updateLastResetDateSql = "UPDATE Settings SET LastResetDate = @LastResetDate WHERE CurrentBonNumber = @CurrentBonNumber";

                            using (SqliteCommand updateCommand = new SqliteCommand(updateLastResetDateSql, _connection))
                            {
                                // Setze die Parameter für das SQL-Statement
                                updateCommand.Parameters.AddWithValue("@LastResetDate", lastResetDate.ToString("yyyy-MM-dd"));
                                updateCommand.Parameters.AddWithValue("@CurrentBonNumber", reader["CurrentBonNumber"]);

                                // Führe das SQL-Statement aus
                                updateCommand.ExecuteNonQuery();
                            }

                            // Optional: Setze hier die Bonnummer zurück, falls erforderlich
                            // currentBonNumber = 1; // Oder ein anderer Startwert
                        }



                    }
                    else
                    {
                        // Fehlerbehandlung, falls kein Eintrag gefunden wurde
                        throw new Exception("Settings entry not found in the database.");
                    }
                }
                _connection.Close();
            }

            return currentBonNumber; // Rückgabe der aktuellen oder zurückgesetzten Bonnummer
        }

        public void ResetBonNumberForTesting()
        {
            string updateSettingsSql = "UPDATE Settings SET CurrentBonNumber = 1, LastResetDate = @LastResetDate";

            _connection.Open();
            using (SqliteCommand updateSettingsCommand = new SqliteCommand(updateSettingsSql, _connection))
            {
                // Setze das LastResetDate auf das aktuelle Datum für Testzwecke
                updateSettingsCommand.Parameters.AddWithValue("@LastResetDate", DateTime.Now.ToString("yyyy-MM-dd"));
                int rowsAffected = updateSettingsCommand.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    // Kein Eintrag wurde aktualisiert, hier könnte eine Fehlerbehandlung erfolgen
                    throw new Exception("No settings entry was updated. Please check if the settings entry exists.");
                }
            }
            _connection.Close();
        }

        public void UpdateCurrentBonNumber(int newNumber)
        {
            string sql = "UPDATE Settings SET CurrentBonNumber = @number";
            _connection.Open();

            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@number", newNumber);
                command.ExecuteNonQuery();
            }
            _connection.Close();
        }


        public async Task UpdateOrderAsync(Order order)
        {
            // Prüfe zuerst, ob die Order und OrderItems nicht null und gültig sind.
            if (order == null || order.OrderItems == null)
            {
                throw new ArgumentNullException(nameof(order), "Die Bestellung und ihre Artikel dürfen nicht null sein.");
            }

            // Öffne eine Transaktion
            await _connection.OpenAsync();
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Aktualisiere die Bestelldaten
                    string updateOrderSql = @"
                UPDATE Orders SET 
                BonNumber = @BonNumber,
                IsDelivery = @IsDelivery,
                PaymentMethod = @PaymentMethod,
                CustomerPhoneNumber = @CustomerPhoneNumber,    
                DeliveryUntil = @DeliveryUntil
                WHERE OrderId = @OrderId";

                    using (var commandOrder = new SqliteCommand(updateOrderSql, _connection, transaction)) // Hier fügst du die Transaktion hinzu
                    {
                        commandOrder.Parameters.AddWithValue("@OrderId", order.OrderId.ToString());
                        commandOrder.Parameters.AddWithValue("@BonNumber", order.BonNumber);
                        commandOrder.Parameters.AddWithValue("@IsDelivery", order.IsDelivery);
                        commandOrder.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod ?? (object)DBNull.Value);
                        commandOrder.Parameters.AddWithValue("@CustomerPhoneNumber", order.CustomerPhoneNumber ?? (object)DBNull.Value);

                        commandOrder.Parameters.AddWithValue("@DeliveryUntil", order.DeliveryUntil ?? (object)DBNull.Value);
                        await commandOrder.ExecuteNonQueryAsync();
                    }

                    // Aktualisiere die OrderItems
                    foreach (var item in order.OrderItems)
                    {
                        string updateItemSql = @"
                    UPDATE OrderItems SET 
                    Gericht = @Gericht,
                    Größe = @Größe,
                    Extras = @Extras,
                    Menge = @Menge,
                    Epreis = @Epreis,
                    Gesamt = @Gesamt,
                    Uhrzeit = @Uhrzeit,
                    LieferungsArt = @LieferungsArt
                    WHERE OrderItemId = @OrderItemId AND OrderId = @OrderId";

                        using (var commandItem = new SqliteCommand(updateItemSql, _connection, transaction)) // Auch hier fügst du die Transaktion hinzu
                        {
                            commandItem.Parameters.AddWithValue("@OrderId", order.OrderId.ToString());
                            commandItem.Parameters.AddWithValue("@OrderItemId", item.OrderItemId);
                            commandItem.Parameters.AddWithValue("@Gericht", item.Gericht ?? (object)DBNull.Value);
                            commandItem.Parameters.AddWithValue("@Größe", item.Größe ?? (object)DBNull.Value);
                            commandItem.Parameters.AddWithValue("@Extras", item.Extras ?? (object)DBNull.Value);
                            commandItem.Parameters.AddWithValue("@Menge", item.Menge);
                            commandItem.Parameters.AddWithValue("@Epreis", item.Epreis);
                            commandItem.Parameters.AddWithValue("@Gesamt", item.Gesamt);
                            commandItem.Parameters.AddWithValue("@Uhrzeit", item.Uhrzeit ?? (object)DBNull.Value);
                            commandItem.Parameters.AddWithValue("@LieferungsArt", item.LieferungsArt);
                            await commandItem.ExecuteNonQueryAsync();
                        }
                    }

                    // Commit der Transaktion, wenn alles erfolgreich war
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Rollback der Transaktion im Fehlerfall
                    transaction.Rollback();

                    // Hier könntest du den Fehler loggen
                    Console.WriteLine("Fehler beim Aktualisieren der Bestellung: " + ex.Message);

                    // Rethrow, um die Fehlerinformationen zu bewahren
                    throw;
                }
                finally
                {
                    // Schließe die Verbindung in einem finally Block,
                    // um sicherzustellen, dass sie auch bei Fehlern geschlossen wird
                    await _connection.CloseAsync();
                }
            }
        }


        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }

        private void InitializeDishes()
        {
            List<Dish> dishes = new List<Dish>
            {
            ////Salate
            //    new Dish { Id = 10, Name = "Bauernsalat", Price = 8.20 , Category = DishCategory.Salad },
            //    new Dish { Id = 11, Name = "Gemischter Salat", Price = 7.90 , Category = DishCategory.Salad },
            //    new Dish { Id = 12, Name = "Cetrioli Salat", Price = 8.20 , Category = DishCategory.Salad},
            //    new Dish { Id = 13, Name = "Tomaten Salat", Price = 7.90 , Category = DishCategory.Salad},
            //    new Dish { Id = 14, Name = "Mozzarella Salat", Price = 8.20 , Category = DishCategory.Salad},
            //    new Dish { Id = 15, Name = "Thunfisch Salat", Price = 8.90 , Category = DishCategory.Salad},
            //    new Dish { Id = 16, Name = "Italia Salat", Price = 9.20 , Category = DishCategory.Salad},
            //    new Dish { Id = 17, Name = "Fisch Salat", Price = 9.20 , Category = DishCategory.Salad},
            //    new Dish { Id = 18, Name = "Chef Salat", Price = 10.20 , Category = DishCategory.Salad},
            //    new Dish { Id = 19, Name = "Chicken-Salat", Price = 9.90 , Category = DishCategory.Salad},
            //    new Dish { Id = 600, Name = "Lachs-Salat", Price = 9.80 , Category = DishCategory.Salad},
            //    new Dish { Id = 601, Name = "Rucola-Salat", Price = 9.80 , Category = DishCategory.Salad},
            //    new Dish { Id = 602, Name = "Döner-Salat", Price = 9.80 , Category = DishCategory.Salad},

            ////Baugette
            //    new Dish { Id = 100, Name = "Käse", Price = 7.30 , Category = DishCategory.Other},
            //    new Dish { Id = 101, Name = "Mozzarella", Price = 8.60 , Category = DishCategory.Other},
            //    new Dish { Id = 102, Name = "Salami", Price = 7.80 , Category = DishCategory.Other},
            //    new Dish { Id = 103, Name = "Schinken", Price = 7.80 , Category = DishCategory.Other},
            //    new Dish { Id = 104, Name = "Thunfisch", Price = 8.80 , Category = DishCategory.Other},
            //    new Dish { Id = 105, Name = "Weißkäse", Price = 9.30 , Category = DishCategory.Other},
            //    new Dish { Id = 106, Name = "Vegetaria", Price = 9.30 , Category = DishCategory.Other},
            //    new Dish { Id = 107, Name = "Chicken", Price = 10.30 , Category = DishCategory.Other},
            //    new Dish { Id = 108, Name = "Ei", Price = 7.80 , Category = DishCategory.Other},
            //    new Dish { Id = 109, Name = "Lachs", Price = 10.30 , Category = DishCategory.Other},
            //    new Dish { Id = 110, Name = "Hawaii", Price = 8.80 , Category = DishCategory.Other},
            //    new Dish { Id = 111, Name = "Dönero", Price = 9.30 , Category = DishCategory.Other},
            //    new Dish { Id = 112, Name = "Knoblauchwurst", Price = 9.30 , Category = DishCategory.Other},
           
            //// Pizzen
            //    new Dish { Id = 20, Name = "Margherita", Price = 6.00 , Category = DishCategory.Pizza},
            //    new Dish { Id = 21, Name = "Champignons", Price = 6.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 22, Name = "Spinat", Price = 6.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 23, Name = "Sardellen", Price = 6.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 24, Name = "Cipolla", Price = 6.20 , Category = DishCategory.Pizza},
            //    new Dish { Id = 25, Name = "Adrinta", Price = 6.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 26, Name = "Salami", Price = 6.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 27, Name = "Schinken", Price = 6.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 28, Name = "Bolognese", Price = 6.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 29, Name = "Vesuvio", Price = 6.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 30, Name = "Funghi", Price = 6.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 31, Name = "Romana", Price = 7.50 , Category = DishCategory.Pizza},
            //    new Dish { Id = 32, Name = "Hawaii", Price = 7.00 , Category = DishCategory.Pizza},
            //    new Dish { Id = 33, Name = "Tonno", Price = 7.50 , Category = DishCategory.Pizza},
            //    new Dish { Id = 34, Name = "Billa", Price = 7.00 , Category = DishCategory.Pizza},
            //    new Dish { Id = 35, Name = "Quattro Stagione", Price = 8.50 , Category = DishCategory.Pizza},
            //    new Dish { Id = 36, Name = "Apollo", Price = 7.70 , Category = DishCategory.Pizza},
            //    new Dish { Id = 37, Name = "Cosa Nostra", Price = 7.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 38, Name = "Primavera", Price = 7.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 39, Name = "Mista", Price = 7.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 40, Name = "Vegetaria", Price = 7.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 41, Name = "Bacio", Price = 7.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 42, Name = "Calzone", Price = 9.60 , Category = DishCategory.Pizza},
            //    new Dish { Id = 43, Name = "Weißkäse", Price = 7.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 44, Name = "Gorgonzola", Price = 7.00 , Category = DishCategory.Pizza},
            //    new Dish { Id = 45, Name = "Riesengarnele", Price = 8.80 , Category = DishCategory.Pizza},
            //    new Dish { Id = 46, Name = "Laguna", Price = 9.80 , Category = DishCategory.Pizza},
            //    new Dish { Id = 47, Name = "Polio", Price = 9.80 , Category = DishCategory.Pizza},
            //    new Dish { Id = 48, Name = "Pollana", Price = 9.80 , Category = DishCategory.Pizza},
            //    new Dish { Id = 49, Name = "Spezial", Price = 9.80 , Category = DishCategory.Pizza},
            //    new Dish { Id = 500, Name = "Capri", Price = 7.00 , Category = DishCategory.Pizza},
            //    new Dish { Id = 501, Name = "Spaghetti", Price = 8.50 , Category = DishCategory.Pizza},
            //    new Dish { Id = 502, Name = "Mexicana", Price = 8.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 503, Name = "Sicilia", Price = 9.80 , Category = DishCategory.Pizza},
            //    new Dish { Id = 504, Name = "Picante", Price = 8.50 , Category = DishCategory.Pizza},
            //    new Dish { Id = 505, Name = "Calzone", Price = 9.60 , Category = DishCategory.Pizza},
            //    new Dish { Id = 506, Name = "Spargel", Price = 8.80 , Category = DishCategory.Pizza},
            //    new Dish { Id = 507, Name = "Döner", Price = 8.90 , Category = DishCategory.Pizza},
            //    new Dish { Id = 508, Name = "Döner", Price = 9.50 , Category = DishCategory.Pizza},
            //    new Dish { Id = 509, Name = "Calzone", Price = 10.30 , Category = DishCategory.Pizza},
            //    new Dish { Id = 510, Name = "Spinat", Price = 7.40 , Category = DishCategory.Pizza},
            //    new Dish { Id = 511, Name = "Polio", Price = 9.50 , Category = DishCategory.Pizza},
            //    new Dish { Id = 512, Name = "Parmenzola", Price = 8.50 , Category = DishCategory.Pizza},
            //    new Dish { Id = 513, Name = "Jalapeno", Price = 9.80 , Category = DishCategory.Pizza},
            //    new Dish { Id = 514, Name = "Melanzana", Price = 9.80 , Category = DishCategory.Pizza},
            };

            AddDishes(dishes);


            //List<Extra> extras = new List<Extra>
            //{
            //    new Extra { Id = 1, Name = "Käse", Price = 1.50 },
            //    new Extra { Id = 2, Name = "Tomatensauce", Price = 1.00 },
            //    new Extra { Id = 3, Name = "Salami", Price = 1.50 },
            //    new Extra { Id = 4, Name = "Schinken", Price = 1.50 },
            //    new Extra { Id = 5, Name = "Pilze", Price = 1.00 },
            //    new Extra { Id = 6, Name = "Oliven", Price = 1.00 },
            //    new Extra { Id = 7, Name = "Zwiebeln", Price = 1.00 },
            //    new Extra { Id = 8, Name = "Paprika", Price = 1.00 },
            //    new Extra { Id = 9, Name = "Pepperoni", Price = 1.50 },
            //    new Extra { Id = 10, Name = "Hähnchen", Price = 2.00 },
            //    new Extra { Id = 11, Name = "Rindfleisch", Price = 2.00 },
            //    new Extra { Id = 12, Name = "Thunfisch", Price = 2.00 },
            //    new Extra { Id = 13, Name = "Ananas", Price = 1.00 },
            //    new Extra { Id = 14, Name = "Spinat", Price = 1.00 },
            //    new Extra { Id = 15, Name = "Pesto", Price = 1.50 },
            //    new Extra { Id = 16, Name = "Parmesan", Price = 1.50 },
            //    new Extra { Id = 17, Name = "Sardellen", Price = 2.00 },
            //    new Extra { Id = 18, Name = "Knoblauch", Price = 1.00 },
            //    new Extra { Id = 19, Name = "Rucola", Price = 1.00 },
            //    new Extra { Id = 20, Name = "Mais", Price = 1.00 }
            //};

            //AddExtras(extras);
        }

    }
}
