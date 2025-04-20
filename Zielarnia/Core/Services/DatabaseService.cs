using MySqlConnector;
using Microsoft.Extensions.Configuration;
using Zielarnia.Data.Models;
using Zielarnia.Core.Models.Base;
using Zielarnia.Core.Models.Enums;
using Zielarnia.UI;
using System.Data.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Zielarnia.Data.Services;

// Zapewnia metody do interakcji z bazą MariaDB.
// Enkapsuluje łączenie z bazą i wykonywanie SQL.
public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
    }

    private MySqlConnection GetConnection() => new MySqlConnection(_connectionString);

    public async Task TestConnectionAsync()
    {
        using var connection = GetConnection();
        try { await connection.OpenAsync(); }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Database connection failed: {ex.Message}"); 
            throw new InvalidOperationException("Failed to connect to the database.", ex);
        }
    }
    private async Task ExecuteNonQueryAsync(string sql, params MySqlParameter[] parameters)
    {
        using var connection = GetConnection();
        try
        {
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }
            await command.ExecuteNonQueryAsync();
        }
        catch (MySqlException)
        {
            throw;
        }
        catch (DbException)
        {
            throw;
        }
    }

    private async Task<object?> ExecuteScalarAsync(string sql, params MySqlParameter[] parameters)
    {
        using var connection = GetConnection();
        try
        {
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }
            var result = await command.ExecuteScalarAsync();

            return (result == DBNull.Value) ? null : result;
        }
        catch (MySqlException)
        {
            throw;
        }
        catch (DbException)
        {
            throw;
        }
    }

    private async Task<List<T>> ReadDataAsync<T>(
        string sql,
        Func<MySqlDataReader, T> mapFunction,
        params MySqlParameter[] parameters)
    {
        var results = new List<T>();
        using var connection = GetConnection();
        MySqlDataReader? reader = null;

        try
        {
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }
            reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                try
                {
                    results.Add(mapFunction(reader));
                }
                catch (Exception ex) when (ex is InvalidCastException || ex is IndexOutOfRangeException || ex is ArgumentException)
                {
                    if (reader != null && !reader.IsClosed)
                    {
                        await reader.CloseAsync();
                    }
                    throw new ApplicationException($"Błąd podczas mapowania danych z bazy. SQL: {sql}", ex);
                }
            }
            if (reader != null && !reader.IsClosed)
            {
                await reader.CloseAsync();
            }
            return results;
        }
        catch (MySqlException)
        {
            if (reader != null && !reader.IsClosed) await reader.CloseAsync();
            throw;
        }
        catch (DbException)
        {
            if (reader != null && !reader.IsClosed) await reader.CloseAsync();
            throw;
        }
    }

    public async Task<Address?> GetAddressByIdAsync(int addressId)
    {
        string sql = "SELECT id, Kraj, miasto, ulica, numer_budynku, numer_mieszkania FROM adresy WHERE id = @Id;";
        var results = await ReadDataAsync(sql, reader => new Address
        {
            Id = reader.GetInt32("id"),
            Country = reader.GetString("Kraj"),
            City = reader.GetString("miasto"),
            Street = reader.GetString("ulica"),
            BuildingNumber = reader.GetString("numer_budynku"),
            ApartmentNumber = reader.IsDBNull(reader.GetOrdinal("numer_mieszkania")) ? null : reader.GetString("numer_mieszkania")
        }, new MySqlParameter("@Id", addressId));
        return results.FirstOrDefault();
    }
    public async Task<int> AddAddressAsync(Address address)
    {
        string sql = "INSERT INTO adresy (Kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES (@Country, @City, @Street, @Building, @Apt); SELECT LAST_INSERT_ID();";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@Country", address.Country),
            new MySqlParameter("@City", address.City),
            new MySqlParameter("@Street", address.Street),
            new MySqlParameter("@Building", address.BuildingNumber),
            new MySqlParameter("@Apt", (object?)address.ApartmentNumber ?? DBNull.Value)
        };
        var result = await ExecuteScalarAsync(sql, parameters);
        if (result == null) throw new ApplicationException("Failed to create address or retrieve last insert ID.");
        return Convert.ToInt32(result);
    }
    public async Task UpdateAddressAsync(Address address)
    {
        string sql = "UPDATE adresy SET Kraj=@Country, miasto=@City, ulica=@Street, numer_budynku=@Building, numer_mieszkania=@Apt WHERE id=@Id;";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@Country", address.Country),
            new MySqlParameter("@City", address.City),
            new MySqlParameter("@Street", address.Street),
            new MySqlParameter("@Building", address.BuildingNumber),
            new MySqlParameter("@Apt", (object?)address.ApartmentNumber ?? DBNull.Value),
            new MySqlParameter("@Id", address.Id)
        };
        await ExecuteNonQueryAsync(sql, parameters);
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        string sql = "SELECT p.id, p.nazwa, p.opis, p.cena, p.kategoria_id, k.nazwa as kategoria_nazwa FROM Produkty p LEFT JOIN Kategorie k ON p.kategoria_id = k.id";
        return await ReadDataAsync(sql, reader =>
        {
            int opisOrdinal = reader.GetOrdinal("opis");
            int kategoriaIdOrdinal = reader.GetOrdinal("kategoria_id");
            return new Product
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("nazwa"),
                Description = reader.IsDBNull(opisOrdinal) ? null : reader.GetString(opisOrdinal),
                Price = reader.GetDecimal("cena"),
                CategoryId = reader.IsDBNull(kategoriaIdOrdinal) ? (int?)null : reader.GetInt32(kategoriaIdOrdinal)
            };
        });
    }
    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        string sql = "SELECT id, nazwa, opis, cena, kategoria_id FROM Produkty WHERE id = @Id;";
        var products = await ReadDataAsync(sql, reader =>
        {
            int opisOrdinal = reader.GetOrdinal("opis");
            int kategoriaIdOrdinal = reader.GetOrdinal("kategoria_id");
            return new Product
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("nazwa"),
                Description = reader.IsDBNull(opisOrdinal) ? null : reader.GetString(opisOrdinal),
                Price = reader.GetDecimal("cena"),
                CategoryId = reader.IsDBNull(kategoriaIdOrdinal) ? (int?)null : reader.GetInt32(kategoriaIdOrdinal)
            };
        }, new MySqlParameter("@Id", productId));
        return products.FirstOrDefault();
    }
    public async Task AddProductAsync(Product product)
    {
        string sql = "INSERT INTO Produkty (nazwa, opis, cena, kategoria_id) VALUES (@Name, @Desc, @Price, @CatId);";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@Name", product.Name),
            new MySqlParameter("@Desc", (object?)product.Description ?? DBNull.Value),
            new MySqlParameter("@Price", product.Price),
            new MySqlParameter("@CatId", (object?)product.CategoryId ?? DBNull.Value)
         };
        await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task UpdateProductAsync(Product product)
    {
        string sql = "UPDATE Produkty SET nazwa = @Name, opis = @Desc, cena = @Price, kategoria_id = @CatId WHERE id = @Id;";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@Name", product.Name),
            new MySqlParameter("@Desc", (object?)product.Description ?? DBNull.Value),
            new MySqlParameter("@Price", product.Price),
            new MySqlParameter("@CatId", (object?)product.CategoryId ?? DBNull.Value),
            new MySqlParameter("@Id", product.Id)
         };
        await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task DeleteProductAsync(int productId)
    {
        string sql = "DELETE FROM Produkty WHERE id = @Id;";
        await ExecuteNonQueryAsync(sql, new MySqlParameter("@Id", productId));
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        string sql = "SELECT id, nazwa FROM Kategorie ORDER BY nazwa;";
        return await ReadDataAsync(sql, reader => new Category { Id = reader.GetInt32("id"), Name = reader.GetString("nazwa") });
    }
    public async Task<Category?> GetCategoryByIdAsync(int categoryId)
    {
        string sql = "SELECT id, nazwa FROM Kategorie WHERE id = @Id;";
        var results = await ReadDataAsync(sql, reader => new Category { Id = reader.GetInt32("id"), Name = reader.GetString("nazwa") }, new MySqlParameter("@Id", categoryId));
        return results.FirstOrDefault();
    }
    public async Task AddCategoryAsync(Category category)
    {
        string sql = "INSERT INTO Kategorie (nazwa) VALUES (@Name);";
        await ExecuteNonQueryAsync(sql, new MySqlParameter("@Name", category.Name));
    }
    public async Task UpdateCategoryAsync(Category category)
    {
        string sql = "UPDATE Kategorie SET nazwa = @Name WHERE id = @Id;";
        await ExecuteNonQueryAsync(sql, new MySqlParameter("@Name", category.Name), new MySqlParameter("@Id", category.Id));
    }
    public async Task DeleteCategoryAsync(int categoryId)
    {
        string sql = "DELETE FROM Kategorie WHERE id = @Id;";
        await ExecuteNonQueryAsync(sql, new MySqlParameter("@Id", categoryId));
    }

    public async Task<List<Client>> GetAllClientsAsync()
    {
        string sql = "SELECT id, imie, nazwisko, email, telefon, adres_id, id_paczkomatu FROM Klienci;";
        return await ReadDataAsync(sql, reader =>
        {
            int paczkomatOrdinal = reader.GetOrdinal("id_paczkomatu");
            return new Client
            {
                Id = reader.GetInt32("id"),
                FirstName = reader.GetString("imie"),
                LastName = reader.GetString("nazwisko"),
                Email = reader.GetString("email"),
                PhoneNumber = reader.GetString("telefon"),
                AddressId = reader.GetInt32("adres_id"),
                ParcelLockerId = reader.IsDBNull(paczkomatOrdinal) ? null : reader.GetString(paczkomatOrdinal)
            };
        });
    }
    public async Task<Client?> GetClientByIdAsync(int clientId)
    {
        string sql = "SELECT id, imie, nazwisko, email, telefon, adres_id, id_paczkomatu FROM Klienci WHERE id = @Id;";
        var results = await ReadDataAsync(sql, reader =>
        {
            int paczkomatOrdinal = reader.GetOrdinal("id_paczkomatu");
            return new Client
            {
                Id = reader.GetInt32("id"),
                FirstName = reader.GetString("imie"),
                LastName = reader.GetString("nazwisko"),
                Email = reader.GetString("email"),
                PhoneNumber = reader.GetString("telefon"),
                AddressId = reader.GetInt32("adres_id"),
                ParcelLockerId = reader.IsDBNull(paczkomatOrdinal) ? null : reader.GetString(paczkomatOrdinal)
            };
        }, new MySqlParameter("@Id", clientId));
        return results.FirstOrDefault();
    }
    public async Task<Client?> GetClientByEmailAsync(string email)
    {
        string sql = "SELECT id, imie, nazwisko, email, telefon, adres_id, id_paczkomatu FROM Klienci WHERE email = @Email LIMIT 1;";
        var results = await ReadDataAsync(sql, reader =>
        {
            int paczkomatOrdinal = reader.GetOrdinal("id_paczkomatu");
            return new Client
            {
                Id = reader.GetInt32("id"),
                FirstName = reader.GetString("imie"),
                LastName = reader.GetString("nazwisko"),
                Email = reader.GetString("email"),
                PhoneNumber = reader.GetString("telefon"),
                AddressId = reader.GetInt32("adres_id"),
                ParcelLockerId = reader.IsDBNull(paczkomatOrdinal) ? null : reader.GetString(paczkomatOrdinal)
            };
        }, new MySqlParameter("@Email", email));
        return results.FirstOrDefault();
    }
    public async Task AddClientAsync(Client client)
    {
        string sql = "INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES (@FName, @LName, @Email, @Phone, @AddrId, @ParcelId);";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@FName", client.FirstName),
            new MySqlParameter("@LName", client.LastName),
            new MySqlParameter("@Email", client.Email),
            new MySqlParameter("@Phone", client.PhoneNumber),
            new MySqlParameter("@AddrId", client.AddressId),
            new MySqlParameter("@ParcelId", (object?)client.ParcelLockerId ?? DBNull.Value)
        };
        await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task UpdateClientAsync(Client client)
    {
        string sql = "UPDATE Klienci SET imie=@FName, nazwisko=@LName, email=@Email, telefon=@Phone, adres_id=@AddrId, id_paczkomatu=@ParcelId WHERE id=@Id;";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@FName", client.FirstName),
            new MySqlParameter("@LName", client.LastName),
            new MySqlParameter("@Email", client.Email),
            new MySqlParameter("@Phone", client.PhoneNumber),
            new MySqlParameter("@AddrId", client.AddressId),
            new MySqlParameter("@ParcelId", (object?)client.ParcelLockerId ?? DBNull.Value),
            new MySqlParameter("@Id", client.Id)
        };
        await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task DeleteClientAsync(int clientId)
    {
        string sql = "DELETE FROM Klienci WHERE id = @Id;";
        await ExecuteNonQueryAsync(sql, new MySqlParameter("@Id", clientId));
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        string sql = @"
            SELECT o.id, o.klient_id, o.data_zamowienia, o.status, k.imie, k.nazwisko
            FROM Zamowienia o
            LEFT JOIN Klienci k ON o.klient_id = k.id
            ORDER BY o.data_zamowienia DESC;";
        return await ReadDataAsync(sql, reader =>
        {
            int klientIdOrdinal = reader.GetOrdinal("klient_id");
            return new Order
            {
                Id = reader.GetInt32("id"),
                ClientId = reader.IsDBNull(klientIdOrdinal) ? null : reader.GetInt32(klientIdOrdinal),
                OrderDate = reader.GetDateTime("data_zamowienia"),
                Status = OrderStatusConverter.FromDbString(reader.GetString("status")),
            };
        });
    }
    public async Task<List<Order>> GetOrdersByStatusAsync(params OrderStatus[] statuses)
    {
        if (statuses == null || !statuses.Any()) return new List<Order>();
        var statusStrings = statuses.Select(OrderStatusConverter.ToDbString).ToList();
        var parameters = new List<MySqlParameter>();
        var statusPlaceholders = new List<string>();
        for (int i = 0; i < statusStrings.Count; i++)
        {
            string paramName = $"@Status{i}";
            statusPlaceholders.Add(paramName);
            parameters.Add(new MySqlParameter(paramName, statusStrings[i]));
        }
        string sql = $@"
            SELECT o.id, o.klient_id, o.data_zamowienia, o.status, k.imie, k.nazwisko
            FROM Zamowienia o
            LEFT JOIN Klienci k ON o.klient_id = k.id
            WHERE o.status IN ({string.Join(",", statusPlaceholders)})
            ORDER BY o.data_zamowienia DESC;";
        return await ReadDataAsync(sql, reader => {
            int klientIdOrdinal = reader.GetOrdinal("klient_id");
            return new Order
            {
                Id = reader.GetInt32("id"),
                ClientId = reader.IsDBNull(klientIdOrdinal) ? null : reader.GetInt32(klientIdOrdinal),
                OrderDate = reader.GetDateTime("data_zamowienia"),
                Status = OrderStatusConverter.FromDbString(reader.GetString("status"))
            };
        }, parameters.ToArray());
    }
    public async Task<List<Order>> GetOrdersByClientIdAsync(int clientId)
    {
        string sql = "SELECT id, klient_id, data_zamowienia, status FROM Zamowienia WHERE klient_id = @ClientId ORDER BY data_zamowienia DESC;";
        return await ReadDataAsync(sql, reader => {
            int klientIdOrdinal = reader.GetOrdinal("klient_id");
            return new Order
            {
                Id = reader.GetInt32("id"),
                ClientId = reader.IsDBNull(klientIdOrdinal) ? null : reader.GetInt32(klientIdOrdinal),
                OrderDate = reader.GetDateTime("data_zamowienia"),
                Status = OrderStatusConverter.FromDbString(reader.GetString("status"))
            };
        }, new MySqlParameter("@ClientId", clientId));
    }
    public async Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId)
    {
        string sql = "SELECT zamowienie_id, produkt_id, ilosc FROM SzczegolyZamowienia WHERE zamowienie_id = @OrderId;";
        return await ReadDataAsync(sql, reader => new OrderDetail
        {
            OrderId = reader.GetInt32("zamowienie_id"),
            ProductId = reader.GetInt32("produkt_id"),
            Quantity = reader.GetInt32("ilosc")
        }, new MySqlParameter("@OrderId", orderId));
    }
    public async Task<int> CreateOrderAsync(Order order)
    {
        string sql = "INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (@ClientId, @OrderDate, @Status); SELECT LAST_INSERT_ID();";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@ClientId", order.ClientId ?? (object)DBNull.Value),
            new MySqlParameter("@OrderDate", order.OrderDate),
            new MySqlParameter("@Status", OrderStatusConverter.ToDbString(order.Status))
        };
        var result = await ExecuteScalarAsync(sql, parameters);
        if (result == null) throw new ApplicationException("Failed to create order or retrieve last insert ID.");
        return Convert.ToInt32(result);
    }
    public async Task AddOrderDetailAsync(OrderDetail detail)
    {
        string sql = "INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (@OrderId, @ProductId, @Quantity);";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@OrderId", detail.OrderId),
            new MySqlParameter("@ProductId", detail.ProductId),
            new MySqlParameter("@Quantity", detail.Quantity)
        };
        await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        string sql = "UPDATE Zamowienia SET status = @Status WHERE id = @Id;";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@Status", OrderStatusConverter.ToDbString(newStatus)),
            new MySqlParameter("@Id", orderId)
        };
        await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task<int> CreateOrderWithDetailsAsync(Order order, List<OrderDetail> details)
    {
        using var connection = GetConnection(); await connection.OpenAsync(); using var transaction = await connection.BeginTransactionAsync();
        try
        {
            string orderSql = "INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (@ClientId, @OrderDate, @Status); SELECT LAST_INSERT_ID();";
            var orderParams = new MySqlParameter[] {
                new MySqlParameter("@ClientId", order.ClientId ?? (object)DBNull.Value),
                new MySqlParameter("@OrderDate", order.OrderDate),
                new MySqlParameter("@Status", OrderStatusConverter.ToDbString(order.Status))
            };
            int orderId;
            using (var orderCommand = new MySqlCommand(orderSql, connection, transaction))
            {
                orderCommand.Parameters.AddRange(orderParams);
                var result = await orderCommand.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value) throw new ApplicationException("Failed to create order header or retrieve ID.");
                orderId = Convert.ToInt32(result);
                order.Id = orderId;
            }
            string detailSql = "INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (@OrderId, @ProductId, @Quantity);";
            foreach (var detail in details)
            {
                if (detail.Quantity <= 0) continue;
                detail.OrderId = orderId;
                var detailParams = new MySqlParameter[] {
                    new MySqlParameter("@OrderId", detail.OrderId),
                    new MySqlParameter("@ProductId", detail.ProductId),
                    new MySqlParameter("@Quantity", detail.Quantity)
                };
                using (var detailCommand = new MySqlCommand(detailSql, connection, transaction))
                {
                    detailCommand.Parameters.AddRange(detailParams);
                    await detailCommand.ExecuteNonQueryAsync();
                }
            }
            await transaction.CommitAsync();
            return orderId;
        }
        catch (Exception) { try { await transaction.RollbackAsync(); } catch (Exception rbEx) { ConsoleHelper.WriteError($"KRYTYCZNY BŁĄD: Nie można wycofać transakcji! {rbEx.Message}"); } throw; }
    }

    public async Task<List<Supplier>> GetAllSuppliersAsync()
    {
        string sql = "SELECT id, nazwa, kontakt, adres_id FROM Dostawcy;";
        return await ReadDataAsync(sql, reader =>
        {
            int kontaktOrdinal = reader.GetOrdinal("kontakt");
            int adresIdOrdinal = reader.GetOrdinal("adres_id");
            return new Supplier
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("nazwa"),
                ContactInfo = reader.IsDBNull(kontaktOrdinal) ? null : reader.GetString(kontaktOrdinal),
                AddressId = reader.IsDBNull(adresIdOrdinal) ? (int?)null : reader.GetInt32(adresIdOrdinal)
            };
        });
    }
    public async Task<Supplier?> GetSupplierByIdAsync(int supplierId)
    {
        string sql = "SELECT id, nazwa, kontakt, adres_id FROM Dostawcy WHERE id = @Id;";
        var results = await ReadDataAsync(sql, reader =>
        {
            int kontaktOrdinal = reader.GetOrdinal("kontakt");
            int adresIdOrdinal = reader.GetOrdinal("adres_id");
            return new Supplier
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("nazwa"),
                ContactInfo = reader.IsDBNull(kontaktOrdinal) ? null : reader.GetString(kontaktOrdinal),
                AddressId = reader.IsDBNull(adresIdOrdinal) ? (int?)null : reader.GetInt32(adresIdOrdinal)
            };
        }, new MySqlParameter("@Id", supplierId));
        return results.FirstOrDefault();
    }
    public async Task AddSupplierAsync(Supplier supplier)
    {
        string sql = "INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES (@Name, @Contact, @AddrId);";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@Name", supplier.Name),
            new MySqlParameter("@Contact", (object?)supplier.ContactInfo ?? DBNull.Value),
            new MySqlParameter("@AddrId", (object?)supplier.AddressId ?? DBNull.Value)
        };
        await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task UpdateSupplierAsync(Supplier supplier)
    {
        string sql = "UPDATE Dostawcy SET nazwa = @Name, kontakt = @Contact, adres_id = @AddrId WHERE id = @Id;";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@Name", supplier.Name),
            new MySqlParameter("@Contact", (object?)supplier.ContactInfo ?? DBNull.Value),
            new MySqlParameter("@AddrId", (object?)supplier.AddressId ?? DBNull.Value),
            new MySqlParameter("@Id", supplier.Id)
        };
        await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task DeleteSupplierAsync(int supplierId)
    {
        string sql = "DELETE FROM Dostawcy WHERE id = @Id;";
        await ExecuteNonQueryAsync(sql, new MySqlParameter("@Id", supplierId));
    }

    public async Task<List<(Supplier Supplier, Product Product)>> GetAllDeliveryLinksAsync()
    {
        string sql = @" SELECT d.dostawca_id, d.produkt_id, s.nazwa as supplier_name, p.nazwa as product_name FROM Dostawy d JOIN Dostawcy s ON d.dostawca_id = s.id JOIN Produkty p ON d.produkt_id = p.id ORDER BY s.nazwa, p.nazwa;";
        var results = new List<(Supplier, Product)>();
        await ReadDataAsync(sql, reader =>
        {
            var supplier = new Supplier { Id = reader.GetInt32("dostawca_id"), Name = reader.GetString("supplier_name") };
            var product = new Product { Id = reader.GetInt32("produkt_id"), Name = reader.GetString("product_name") };
            results.Add((supplier, product));
            return true;
        });
        return results;
    }
    public async Task AddDeliveryLinkAsync(int supplierId, int productId)
    {
        string sql = "INSERT IGNORE INTO Dostawy (dostawca_id, produkt_id) VALUES (@SupplierId, @ProductId);";
        await ExecuteNonQueryAsync(sql, new MySqlParameter("@SupplierId", supplierId), new MySqlParameter("@ProductId", productId));
    }
    public async Task RemoveDeliveryLinkAsync(int supplierId, int productId)
    {
        string sql = "DELETE FROM Dostawy WHERE dostawca_id = @SupplierId AND produkt_id = @ProductId;";
        await ExecuteNonQueryAsync(sql, new MySqlParameter("@SupplierId", supplierId), new MySqlParameter("@ProductId", productId));
    }

    public async Task<List<BillType>> GetAllBillTypesAsync()
    {
        string sql = "SELECT id, typ, opis FROM typy_rachunkow ORDER BY typ;";
        return await ReadDataAsync(sql, reader =>
        {
            int opisOrdinal = reader.GetOrdinal("opis");
            return new BillType
            {
                Id = reader.GetInt32("id"),
                TypeName = reader.GetString("typ"),
                Description = reader.IsDBNull(opisOrdinal) ? null : reader.GetString(opisOrdinal)
            };
        });
    }
    public async Task<BillType?> GetBillTypeByIdAsync(int billTypeId)
    {
        string sql = "SELECT id, typ, opis FROM typy_rachunkow WHERE id = @Id;";
        var results = await ReadDataAsync(sql, reader =>
        {
            int opisOrdinal = reader.GetOrdinal("opis");
            return new BillType
            {
                Id = reader.GetInt32("id"),
                TypeName = reader.GetString("typ"),
                Description = reader.IsDBNull(opisOrdinal) ? null : reader.GetString(opisOrdinal)
            };
        }, new MySqlParameter("@Id", billTypeId));
        return results.FirstOrDefault();
    }
    public async Task<BillType?> GetBillTypeByNameAsync(string typeName)
    {
        string sql = "SELECT id, typ, opis FROM typy_rachunkow WHERE typ = @Name LIMIT 1;";
        var results = await ReadDataAsync(sql, reader =>
        {
            int opisOrdinal = reader.GetOrdinal("opis");
            return new BillType
            {
                Id = reader.GetInt32("id"),
                TypeName = reader.GetString("typ"),
                Description = reader.IsDBNull(opisOrdinal) ? null : reader.GetString(opisOrdinal)
            };
        }, new MySqlParameter("@Name", typeName));
        return results.FirstOrDefault();
    }
    public async Task AddBillTypeAsync(BillType billType)
    {
        string sql = "INSERT INTO typy_rachunkow (typ, opis) VALUES (@Type, @Desc);";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@Type", billType.TypeName),
            new MySqlParameter("@Desc", (object?)billType.Description ?? DBNull.Value)
        };
        await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task UpdateBillTypeAsync(BillType billType)
    {
        string sql = "UPDATE typy_rachunkow SET typ = @Type, opis = @Desc WHERE id = @Id;";
        var parameters = new MySqlParameter[] {
             new MySqlParameter("@Type", billType.TypeName),
             new MySqlParameter("@Desc", (object?)billType.Description ?? DBNull.Value),
             new MySqlParameter("@Id", billType.Id)
         };
        await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task DeleteBillTypeAsync(int billTypeId)
    {
        string sql = "DELETE FROM typy_rachunkow WHERE id = @Id;";
        await ExecuteNonQueryAsync(sql, new MySqlParameter("@Id", billTypeId));
    }

    public async Task<List<Bill>> GetAllBillsAsync()
    {
        string sql = @"
            SELECT r.id, r.data, r.typ as bill_type_id, r.kwota, t.typ as bill_type_name
            FROM rachunki r
            LEFT JOIN typy_rachunkow t ON r.typ = t.id
            ORDER BY r.data DESC;";
        return await ReadDataAsync(sql, reader =>
        {
            int typeIdOrdinal = reader.GetOrdinal("bill_type_id");
            return new Bill
            {
                Id = reader.GetInt32("id"),
                BillDate = DateOnly.FromDateTime(reader.GetDateTime("data")),
                BillTypeId = reader.IsDBNull(typeIdOrdinal) ? 0 : reader.GetInt32(typeIdOrdinal),
                Amount = reader.GetDecimal("kwota")
            };
        });
    }
    public async Task<Bill?> GetBillByIdAsync(int billId)
    {
        string sql = "SELECT id, data, typ, kwota FROM rachunki WHERE id = @Id;";
        var results = await ReadDataAsync(sql, reader => new Bill
        {
            Id = reader.GetInt32("id"),
            BillDate = DateOnly.FromDateTime(reader.GetDateTime("data")),
            BillTypeId = reader.GetInt32("typ"),
            Amount = reader.GetDecimal("kwota")
        }, new MySqlParameter("@Id", billId));
        return results.FirstOrDefault();
    }
    public async Task AddBillAsync(Bill bill)
    {
        DateTime billDateTime = bill.BillDate.ToDateTime(TimeOnly.MinValue);
        string sql = "INSERT INTO rachunki (data, typ, kwota) VALUES (@Date, @TypeId, @Amount);";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@Date", billDateTime),
            new MySqlParameter("@TypeId", bill.BillTypeId),
            new MySqlParameter("@Amount", bill.Amount)
        };
        await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task UpdateBillAsync(Bill bill)
    {
        DateTime billDateTime = bill.BillDate.ToDateTime(TimeOnly.MinValue);
        string sql = "UPDATE rachunki SET data = @Date, typ = @TypeId, kwota = @Amount WHERE id = @Id;";
        var parameters = new MySqlParameter[] {
            new MySqlParameter("@Date", billDateTime),
            new MySqlParameter("@TypeId", bill.BillTypeId),
            new MySqlParameter("@Amount", bill.Amount),
            new MySqlParameter("@Id", bill.Id)
        };
        await ExecuteNonQueryAsync(sql, parameters);
    }

}