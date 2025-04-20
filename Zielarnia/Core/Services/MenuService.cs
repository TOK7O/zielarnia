using Zielarnia.Core.Models;
using Zielarnia.Core.Models.Enums;
using Zielarnia.Data.Services;
using Zielarnia.UI;
using Zielarnia.Core.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Zielarnia.Data.Models;
using System.Globalization;
using MySqlConnector;

namespace Zielarnia.Core.Services
{
    // Klasa przechowująca wszystkie metody służące do wyświetlania i interakcji użytkownika przez konsolę z bazą danych
    public class MenuService
    {
        private readonly DatabaseService _dbService;
        private readonly ILoggerService _logger;

        public MenuService(DatabaseService dbService, ILoggerService logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        public async Task ShowMainMenu(User user)
        {
            bool keepRunning = true;
            while (keepRunning)
            {
                ConsoleHelper.ClearScreen();
                ConsoleHelper.DisplayHeader($"Menu Główne ({user.Role})");
                Console.WriteLine(" 1. Wyświetl produkty");
                switch (user.Role)
                {
                    case UserRole.Client: DisplayClientOptions(); break;
                    case UserRole.Herbalist: DisplayHerbalistOptions(); break;
                    case UserRole.Admin: DisplayAdminOptions(); break;
                }
                Console.WriteLine("--------------------");
                Console.WriteLine(" 0. Wyloguj i zakończ");
                Console.WriteLine("--------------------");
                int choice = ConsoleHelper.ReadInt("Wybierz opcję:");
                await _logger.LogInfoAsync($"User '{user.Login}' chose menu option: {choice} (Role: {user.Role})");
                try
                {
                    switch (user.Role)
                    {
                        case UserRole.Client: keepRunning = await HandleClientChoice(choice, user); break;
                        case UserRole.Herbalist: keepRunning = await HandleHerbalistChoice(choice, user); break;
                        case UserRole.Admin: keepRunning = await HandleAdminChoice(choice, user); break;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteError($"Wystąpił nieoczekiwany błąd: {ex.Message}");
                    await _logger.LogErrorAsync($"Error during menu action for user '{user.Login}', choice {choice}.", ex);
                    keepRunning = true;
                    Console.WriteLine("\nNaciśnij dowolny klawisz..."); Console.ReadKey();
                }

                if (keepRunning)
                {
                    Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić do menu głównego...");
                    Console.ReadKey();
                }
            }
            await _logger.LogInfoAsync($"User '{user.Login}' logged out.");
        }

        private void DisplayClientOptions()
        {
            Console.WriteLine(" 2. Złóż nowe zamówienie");
            Console.WriteLine(" 3. Wyświetl moje zamówienia");
        }
        private void DisplayHerbalistOptions()
        {
            Console.WriteLine("--- Zarządzanie Zielarnią ---");
            Console.WriteLine("10. Zarządzaj produktami");
            Console.WriteLine("11. Zarządzaj kategoriami");
            Console.WriteLine("12. Zarządzaj klientami");
            Console.WriteLine("13. Zarządzaj zamówieniami");
            Console.WriteLine("14. Zarządzaj dostawcami");
            Console.WriteLine("15. Zarządzaj powiązaniami dostawców");
            Console.WriteLine("16. Wyświetl wszystkie zamówienia");
        }
        private void DisplayAdminOptions()
        {
            DisplayHerbalistOptions();
            Console.WriteLine("--- Administracja ---");
            Console.WriteLine("20. Zarządzaj typami rachunków");
            Console.WriteLine("21. Zarządzaj rachunkami");
        }

        private async Task<bool> HandleClientChoice(int choice, User user)
        {
            switch (choice)
            {
                case 1: await ShowAllProducts(user); return true;
                case 2: await PlaceNewOrder(user); return true;
                case 3: await ShowMyOrders(user); return true;
                case 0: return false;
                default: ConsoleHelper.WriteWarning("Nieznana opcja."); return true;
            }
        }
        private async Task<bool> HandleHerbalistChoice(int choice, User user)
        {
            if (choice >= 1 && choice <= 3) return await HandleClientChoice(choice, user);
            if (choice == 0) return false;

            switch (choice)
            {
                case 10: await ManageProducts(user); return true;
                case 11: await ManageCategories(user); return true;
                case 12: await ManageClients(user); return true;
                case 13: await ManageOrders(user); return true;
                case 14: await ManageSuppliers(user); return true;
                case 15: await ManageDeliveryLinks(user); return true;
                case 16: await ShowAllOrders(user, true); break;
                default: ConsoleHelper.WriteWarning("Nieznana opcja."); return true;
            }
            return true;
        }
        private async Task<bool> HandleAdminChoice(int choice, User user)
        {
            if ((choice >= 1 && choice <= 3) || (choice >= 10 && choice <= 16)) return await HandleHerbalistChoice(choice, user);
            if (choice == 0) return false;

            switch (choice)
            {
                case 20: await ManageBillTypes(user); return true;
                case 21: await ManageBills(user); return true;
                default: ConsoleHelper.WriteWarning("Nieznana opcja."); return true;
            }
        }

        private async Task ShowAllProducts(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: ShowAllProducts");
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DisplayHeader("Lista Produktów");
            try
            {
                var products = await _dbService.GetAllProductsAsync();
                if (!products.Any())
                {
                    Console.WriteLine("Brak produktów w bazie.");
                    return;
                }
                Console.WriteLine($"{"ID",-5} | {"Nazwa",-45} | {"Cena",-12} | {"Kat.ID",-7} | {"Opis"}");
                Console.WriteLine(new string('-', 90));
                foreach (var p in products)
                {
                    string categoryInfo = p.CategoryId.HasValue ? p.CategoryId.Value.ToString() : "Brak";
                    string description = p.Description ?? "Brak";
                    if (description.Length > 30) description = description.Substring(0, 27) + "...";
                    Console.WriteLine($"{p.Id,-5} | {p.Name,-45} | {p.Price,-12:C} | {categoryInfo,-7} | {description}");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Nie można pobrać listy produktów. {ex.Message}");
                await _logger.LogErrorAsync($"Failure in ShowAllProducts for user '{user.Login}'.", ex);
            }
        }

        private async Task PlaceNewOrder(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: PlaceNewOrder");
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DisplayHeader("Składanie nowego zamówienia");
            try
            {
                var client = await _dbService.GetClientByEmailAsync(user.Login);
                if (client == null) { ConsoleHelper.WriteError("Nie znaleziono profilu klienta."); await _logger.LogWarningAsync($"Client profile not found for user '{user.Login}'."); return; }

                var products = await _dbService.GetAllProductsAsync();
                if (!products.Any()) { Console.WriteLine("Brak dostępnych produktów."); return; }

                Console.WriteLine("Dostępne produkty:");
                Console.WriteLine($"{"ID",-5} | {"Nazwa",-50} | {"Cena",-12}");
                Console.WriteLine(new string('-', 60));
                products.ForEach(p => Console.WriteLine($"{p.Id,-5} | {p.Name,-50} | {p.Price,-12:C}"));
                Console.WriteLine(new string('-', 60));

                var orderDetails = new List<OrderDetail>();
                bool addMore = true;
                while (addMore)
                {
                    int productId = ConsoleHelper.ReadInt("Podaj ID produktu (lub 0 aby zakończyć):", 0);
                    if (productId == 0) { addMore = false; continue; }
                    var selectedProduct = products.FirstOrDefault(p => p.Id == productId);
                    if (selectedProduct == null) { ConsoleHelper.WriteWarning("Nie znaleziono produktu o podanym ID."); continue; }
                    int quantity = ConsoleHelper.ReadInt($"Podaj ilość dla '{selectedProduct.Name}':", 1);
                    var existingDetail = orderDetails.FirstOrDefault(d => d.ProductId == productId);
                    // Jeśli produkt już jest na liście, zwiększ ilość, w przeciwnym razie dodaj nową pozycję.
                    if (existingDetail != null) { existingDetail.Quantity += quantity; Console.WriteLine($"Zaktualizowano ilość dla '{selectedProduct.Name}' do {existingDetail.Quantity}."); }
                    else { orderDetails.Add(new OrderDetail { ProductId = productId, Quantity = quantity }); Console.WriteLine($"Dodano '{selectedProduct.Name}' (ilość: {quantity}) do zamówienia."); }
                }

                if (!orderDetails.Any()) { Console.WriteLine("Anulowano składanie zamówienia (brak produktów)."); return; }

                Console.WriteLine("\n--- Podsumowanie zamówienia ---");
                decimal totalAmount = 0;
                foreach (var detail in orderDetails)
                {
                    var product = products.First(p => p.Id == detail.ProductId);
                    Console.WriteLine($"- {product.Name} x {detail.Quantity} = {product.Price * detail.Quantity:C}");
                    totalAmount += product.Price * detail.Quantity;
                }
                Console.WriteLine($"------------------------------\nSuma: {totalAmount:C}");

                if (!ConsoleHelper.Confirm("Czy chcesz złożyć to zamówienie?")) { Console.WriteLine("Anulowano składanie zamówienia.");
                await _logger.LogInfoAsync($"User '{user.Login}' cancelled order placement."); return; }

                var newOrder = new Order { ClientId = client.Id, OrderDate = DateTime.Now, Status = OrderStatus.Nowe };
                int newOrderId = await _dbService.CreateOrderWithDetailsAsync(newOrder, orderDetails);
                ConsoleHelper.WriteSuccess($"Zamówienie nr {newOrderId} zostało pomyślnie złożone!");
                await _logger.LogInfoAsync($"User '{user.Login}' placed order ID {newOrderId} for client ID {client.Id}. Total: {totalAmount:C}");
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Błąd podczas składania zamówienia: {ex.Message}"); await _logger.LogErrorAsync($"Error during PlaceNewOrder for user '{user.Login}'.", ex); }
        }

        private async Task ShowMyOrders(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: ShowMyOrders");
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DisplayHeader("Moje Zamówienia");
            bool detailsShown = false;
            try
            {
                var client = await _dbService.GetClientByEmailAsync(user.Login);
                if (client == null) { ConsoleHelper.WriteError("Nie znaleziono profilu klienta."); return; }
                var orders = await _dbService.GetOrdersByClientIdAsync(client.Id);
                if (!orders.Any()) { Console.WriteLine("Nie masz jeszcze żadnych zamówień."); return; }
                Console.WriteLine($"{"ID Zam.",-10} | {"Data",-20} | {"Status",-15} | Szczegóły?");
                Console.WriteLine(new string('-', 60));
                foreach (var order in orders) { Console.WriteLine($"{order.Id,-10} | {order.OrderDate,-20:yyyy-MM-dd HH:mm} | {order.Status,-15} | (Wpisz ID)"); }
                Console.WriteLine(new string('-', 60));
                int orderIdToShow = ConsoleHelper.ReadInt("Podaj ID zamówienia do szczegółów (lub 0):", 0);
                if (orderIdToShow > 0)
                {
                    if (orders.Any(o => o.Id == orderIdToShow))
                    {
                        await ShowOrderDetails(orderIdToShow);
                        detailsShown = true;
                    }
                    else { ConsoleHelper.WriteWarning("Nie masz zamówienia o podanym ID."); }
                }
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Nie można pobrać zamówień: {ex.Message}"); await _logger.LogErrorAsync($"Error showing orders for user '{user.Login}'.", ex); }

            if (detailsShown)
            {
                Console.WriteLine("\nNaciśnij klawisz, aby wrócić do menu...");
                Console.ReadKey();
            }
        }

        private async Task ManageProducts(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: ManageProducts");
            bool stayInMenu = true;
            while (stayInMenu)
            {
                ConsoleHelper.ClearScreen();
                ConsoleHelper.DisplayHeader("Zarządzanie Produktami");
                Console.WriteLine("1. Wyświetl wszystkie produkty");
                Console.WriteLine("2. Dodaj nowy produkt");
                Console.WriteLine("3. Edytuj produkt");
                Console.WriteLine("4. Usuń produkt");
                Console.WriteLine("0. Wróć do menu głównego");
                Console.WriteLine("---------------------------");
                int choice = ConsoleHelper.ReadInt("Wybierz opcję:", 0, 4);
                bool actionRequiresPause = (choice == 1);

                switch (choice)
                {
                    case 1: await ShowAllProducts(user); break;
                    case 2: await AddNewProduct(user); break;
                    case 3: await EditProduct(user); break;
                    case 4: await DeleteProduct(user); break;
                    case 0: stayInMenu = false; actionRequiresPause = false; break;
                    default: ConsoleHelper.WriteWarning("Nieznana opcja."); actionRequiresPause = false; break;
                }
                if (stayInMenu && actionRequiresPause)
                {
                    Console.WriteLine("\nNaciśnij klawisz by wrócić do podmenu...");
                    Console.ReadKey();
                }
                else if (stayInMenu)
                {
                    Console.WriteLine("\nNaciśnij klawisz...");
                    Console.ReadKey();
                }
            }
        }
        private async Task AddNewProduct(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' attempting to add product.");
            ConsoleHelper.DisplayHeader("Dodawanie Nowego Produktu");
            try
            {
                string name = ConsoleHelper.ReadString("Nazwa produktu:");
                string? description = ConsoleHelper.ReadString("Opis (opcjonalnie):", true);
                decimal price = ConsoleHelper.ReadDecimal("Cena (np. 10.99):", 0.01m);

                var categories = await _dbService.GetAllCategoriesAsync();
                Console.WriteLine("Dostępne kategorie:");
                Console.WriteLine(" 0. Brak kategorii");
                categories.ForEach(c => Console.WriteLine($" {c.Id}. {c.Name}"));

                int categoryChoice = ConsoleHelper.ReadInt("Wybierz ID Kategorii:", 0);
                int? categoryId = null;
                if (categoryChoice > 0 && categories.Any(c => c.Id == categoryChoice))
                {
                    categoryId = categoryChoice;
                }
                else if (categoryChoice != 0)
                {
                    ConsoleHelper.WriteWarning("Wybrano nieprawidłowe ID, ustawiono Brak.");
                }

                var newProduct = new Product
                {
                    Name = name,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description,
                    Price = price,
                    CategoryId = categoryId
                };

                await _dbService.AddProductAsync(newProduct);
                ConsoleHelper.WriteSuccess($"Produkt '{name}' został dodany.");
                await _logger.LogInfoAsync($"User '{user.Login}' added product '{name}'.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Nie można dodać produktu: {ex.Message}");
                await _logger.LogErrorAsync($"Error adding product by user '{user.Login}'.", ex);
            }
        }

        private async Task EditProduct(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' attempting to edit product.");
            ConsoleHelper.DisplayHeader("Edycja Produktu");
            await ShowAllProducts(user);
            Console.WriteLine("---");
            int productId = ConsoleHelper.ReadInt("Podaj ID produktu do edycji (0=anuluj):", 0);
            if (productId == 0) return;

            try
            {
                var product = await _dbService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    ConsoleHelper.WriteWarning($"Nie znaleziono produktu o ID: {productId}");
                    return;
                }

                Console.WriteLine($"Edytujesz: {product.Name} (Cena: {product.Price:C}, Kategoria ID: {product.CategoryId?.ToString() ?? "Brak"})");
                string name = ConsoleHelper.ReadString($"Nowa nazwa (Enter by zostawić '{product.Name}'):", true);
                string? description = ConsoleHelper.ReadString($"Nowy opis (Enter by zostawić):", true);
                string priceStr = ConsoleHelper.ReadString($"Nowa cena (Enter by zostawić '{product.Price:C}'):", true);

                var categories = await _dbService.GetAllCategoriesAsync();
                Console.WriteLine("Dostępne kategorie (obecna: {0}):", product.CategoryId?.ToString() ?? "Brak");
                Console.WriteLine(" 0. Brak kategorii");
                categories.ForEach(c => Console.WriteLine($" {c.Id}. {c.Name}"));
                string categoryStr = ConsoleHelper.ReadString($"Nowe ID Kategorii (Enter by zostawić):", true);

                bool changed = false;
                if (!string.IsNullOrWhiteSpace(name) && !name.Equals(product.Name, StringComparison.OrdinalIgnoreCase))
                {
                    product.Name = name;
                    changed = true;
                }
                if (description != null)
                {
                    string? newDesc = string.IsNullOrWhiteSpace(description) ? null : description;
                    if (newDesc != product.Description)
                    {
                        product.Description = newDesc;
                        changed = true;
                    }
                }
                if (decimal.TryParse(priceStr?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal newPrice) && newPrice > 0 && newPrice != product.Price)
                {
                    product.Price = newPrice;
                    changed = true;
                }
                if (int.TryParse(categoryStr, out int newCatIdChoice))
                {
                    int? newCatId = newCatIdChoice == 0 ? null : (int?)newCatIdChoice;
                    if (newCatId != product.CategoryId)
                    {
                        if (newCatId == null || categories.Any(c => c.Id == newCatId.Value))
                        {
                            product.CategoryId = newCatId;
                            changed = true;
                        }
                        else
                        {
                            ConsoleHelper.WriteWarning("Nie zmieniono kategorii - podano nieprawidłowe ID.");
                        }
                    }
                }

                if (changed)
                {
                    await _dbService.UpdateProductAsync(product);
                    ConsoleHelper.WriteSuccess($"Produkt ID {productId} został zaktualizowany.");
                    await _logger.LogInfoAsync($"User '{user.Login}' updated product ID {productId}.");
                }
                else
                {
                    Console.WriteLine("Nie wprowadzono zmian.");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Nie można edytować produktu: {ex.Message}");
                await _logger.LogErrorAsync($"Error editing product ID {productId} by user '{user.Login}'.", ex);
            }
        }

        private async Task DeleteProduct(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' attempting to delete product.");
            ConsoleHelper.DisplayHeader("Usuwanie Produktu");
            await ShowAllProducts(user);
            Console.WriteLine("---");
            int productId = ConsoleHelper.ReadInt("Podaj ID produktu do usunięcia (0=anuluj):", 0);
            if (productId == 0) return;
            try
            {
                var product = await _dbService.GetProductByIdAsync(productId);
                if (product == null) { ConsoleHelper.WriteWarning($"Nie znaleziono produktu o ID: {productId}"); return; }
                if (ConsoleHelper.Confirm($"Usunąć '{product.Name}' (ID: {productId})? Może to wpłynąć na zamówienia!"))
                {
                    await _dbService.DeleteProductAsync(productId);
                    ConsoleHelper.WriteSuccess($"Produkt ID {productId} usunięty.");
                    await _logger.LogInfoAsync($"User '{user.Login}' deleted product ID {productId} ('{product.Name}').");
                }
                else { Console.WriteLine("Anulowano."); await _logger.LogInfoAsync($"User '{user.Login}' cancelled deletion of product ID {productId}."); }
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Nie można usunąć produktu: {ex.Message}"); await _logger.LogErrorAsync($"Error deleting product ID {productId} by user '{user.Login}'.", ex); }
        }

        private async Task ManageCategories(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: ManageCategories");
            bool stayInMenu = true;
            while (stayInMenu)
            {
                ConsoleHelper.ClearScreen(); ConsoleHelper.DisplayHeader("Zarządzanie Kategoriami");
                Console.WriteLine("1. Lista"); Console.WriteLine("2. Dodaj");
                Console.WriteLine("3. Edytuj"); Console.WriteLine("4. Usuń");
                Console.WriteLine("0. Wróć"); Console.WriteLine("-------------");
                int choice = ConsoleHelper.ReadInt("Wybierz:", 0, 4);
                bool actionPaused = (choice == 1);
                switch (choice)
                {
                    case 1: await ListCategories(user, true); break;
                    case 2: await AddNewCategory(user); break;
                    case 3: await EditCategory(user); break;
                    case 4: await DeleteCategory(user); break;
                    case 0: stayInMenu = false; break;
                }
                if (stayInMenu && !actionPaused) { Console.WriteLine("\nNaciśnij..."); Console.ReadKey(); }
                else if (stayInMenu && actionPaused) { Console.WriteLine("\nNaciśnij by wrócić..."); Console.ReadKey(); }
            }
        }
        private async Task ListCategories(User user, bool pause = true)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' list categories.");
            ConsoleHelper.DisplayHeader("Lista Kategorii");
            try
            {
                var categories = await _dbService.GetAllCategoriesAsync();
                if (!categories.Any()) { Console.WriteLine("Brak kategorii."); return; }
                Console.WriteLine($"{"ID",-5} | {"Nazwa"}"); Console.WriteLine(new string('-', 30));
                categories.ForEach(c => Console.WriteLine($"{c.Id,-5} | {c.Name}"));
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Błąd: {ex.Message}"); await _logger.LogErrorAsync($"Error list categories by '{user.Login}'.", ex); }
        }
        private async Task AddNewCategory(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' add category.");
            ConsoleHelper.DisplayHeader("Dodawanie Kategorii");
            try
            {
                string name = ConsoleHelper.ReadString("Nazwa:");
                var newCategory = new Category { Name = name };
                await _dbService.AddCategoryAsync(newCategory);
                ConsoleHelper.WriteSuccess($"Kategoria '{name}' dodana.");
                await _logger.LogInfoAsync($"User '{user.Login}' added category '{name}'.");
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Błąd: {ex.Message}"); await _logger.LogErrorAsync($"Error add category by '{user.Login}'.", ex); }
        }
        private async Task EditCategory(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' edit category.");
            ConsoleHelper.DisplayHeader("Edycja Kategorii");
            await ListCategories(user, false);
            int categoryId = ConsoleHelper.ReadInt("\nID kategorii (0=anuluj):", 0);
            if (categoryId == 0) return;
            try
            {
                var category = await _dbService.GetCategoryByIdAsync(categoryId);
                if (category == null) { ConsoleHelper.WriteWarning($"Nie znaleziono ID: {categoryId}"); return; }
                string newName = ConsoleHelper.ReadString($"Nowa nazwa ('{category.Name}'):", true);
                if (!string.IsNullOrWhiteSpace(newName) && !newName.Equals(category.Name, StringComparison.OrdinalIgnoreCase))
                {
                    category.Name = newName;
                    await _dbService.UpdateCategoryAsync(category);
                    ConsoleHelper.WriteSuccess($"Kategoria ID {categoryId} zaktualizowana.");
                    await _logger.LogInfoAsync($"User '{user.Login}' updated category {categoryId}.");
                }
                else { Console.WriteLine("Nie zmieniono."); }
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Błąd: {ex.Message}"); await _logger.LogErrorAsync($"Error edit category {categoryId} by '{user.Login}'.", ex); }
        }
        private async Task DeleteCategory(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' delete category.");
            ConsoleHelper.DisplayHeader("Usuwanie Kategorii");
            await ListCategories(user, false);
            int categoryId = ConsoleHelper.ReadInt("\nID kategorii (0=anuluj):", 0);
            if (categoryId == 0) return;
            try
            {
                var category = await _dbService.GetCategoryByIdAsync(categoryId);
                if (category == null) { ConsoleHelper.WriteWarning($"Nie znaleziono ID: {categoryId}"); return; }
                if (ConsoleHelper.Confirm($"Usunąć '{category.Name}'?"))
                {
                    await _dbService.DeleteCategoryAsync(categoryId);
                    ConsoleHelper.WriteSuccess($"Kategoria ID {categoryId} usunięta.");
                    await _logger.LogInfoAsync($"User '{user.Login}' deleted category {categoryId}.");
                }
                else { Console.WriteLine("Anulowano."); await _logger.LogInfoAsync($"User '{user.Login}' cancelled delete category {categoryId}."); }
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Błąd: {ex.Message}"); await _logger.LogErrorAsync($"Error delete category {categoryId} by '{user.Login}'.", ex); }
        }

        private async Task ManageClients(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: ManageClients");
            bool stayInMenu = true;
            while (stayInMenu)
            {
                ConsoleHelper.ClearScreen(); ConsoleHelper.DisplayHeader("Zarządzanie Klientami");
                Console.WriteLine("1. Lista"); Console.WriteLine("2. Dodaj");
                Console.WriteLine("3. Edytuj"); Console.WriteLine("4. Usuń");
                Console.WriteLine("5. Pokaż adres"); Console.WriteLine("0. Wróć");
                Console.WriteLine("-------------");
                int choice = ConsoleHelper.ReadInt("Wybierz:", 0, 5);
                bool actionPaused = (choice == 1 || choice == 5);
                switch (choice)
                {
                    case 1: await ListClients(user, true); break;
                    case 2: await AddNewClient(user); break;
                    case 3: await EditClient(user); break;
                    case 4: await DeleteClient(user); break;
                    case 5: await ShowClientAddress(user); break;
                    case 0: stayInMenu = false; break;
                }
                if (stayInMenu && !actionPaused) { Console.WriteLine("\nNaciśnij..."); Console.ReadKey(); }
                else if (stayInMenu && actionPaused) { Console.WriteLine("\nNaciśnij by wrócić..."); Console.ReadKey(); }
            }
        }
        private async Task ListClients(User user, bool pause = true)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' list clients.");
            ConsoleHelper.DisplayHeader("Lista Klientów");
            try
            {
                var clients = await _dbService.GetAllClientsAsync();
                if (!clients.Any())
                {
                    Console.WriteLine("Brak klientów.");
                    return;
                }
                Console.WriteLine($"{"ID",-5} | {"Imię",-15} | {"Nazwisko",-20} | {"Email",-35} | {"Telefon",-20} | AdrID | Paczkomat");
                Console.WriteLine(new string('-', 105));
                clients.ForEach(c => Console.WriteLine($"{c.Id,-5} | {c.FirstName,-15} | {c.LastName,-20} | {c.Email,-35} | {c.PhoneNumber,-20} | {c.AddressId,-5} | {c.ParcelLockerId ?? "Brak"}"));
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error list clients by '{user.Login}'.", ex);
            }
        }

        private async Task AddNewClient(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' add client.");
            ConsoleHelper.DisplayHeader("Dodawanie Klienta");
            try
            {
                Console.WriteLine("Adres:");
                var newAddress = ReadAddressFromConsole();
                int addressId = await _dbService.AddAddressAsync(newAddress);
                ConsoleHelper.WriteSuccess($"Adres ID: {addressId} dodany.");

                Console.WriteLine("\nDane klienta:");
                string fname = ConsoleHelper.ReadString("Imię:");
                string lname = ConsoleHelper.ReadString("Nazwisko:");
                string email = ConsoleHelper.ReadString("Email:");
                string phone = ConsoleHelper.ReadString("Telefon:");
                string parcelLocker = ConsoleHelper.ReadString("Paczkomat:", true);

                var newClient = new Client
                {
                    FirstName = fname,
                    LastName = lname,
                    Email = email,
                    PhoneNumber = phone,
                    AddressId = addressId,
                    ParcelLockerId = string.IsNullOrWhiteSpace(parcelLocker) ? null : parcelLocker
                };

                await _dbService.AddClientAsync(newClient);
                ConsoleHelper.WriteSuccess($"Klient '{fname} {lname}' dodany.");
                await _logger.LogInfoAsync($"User '{user.Login}' added client '{fname}'.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error add client by '{user.Login}'.", ex);
            }
        }

        private Address ReadAddressFromConsole(Address? existingAddress = null)
        {
            Console.WriteLine($"Kraj ({existingAddress?.Country ?? "brak"}):");
            string country = ConsoleHelper.ReadString("Podaj:", existingAddress == null || string.IsNullOrEmpty(existingAddress.Country));

            Console.WriteLine($"Miasto ({existingAddress?.City ?? "brak"}):");
            string city = ConsoleHelper.ReadString("Podaj:", existingAddress == null || string.IsNullOrEmpty(existingAddress.City));

            Console.WriteLine($"Ulica ({existingAddress?.Street ?? "brak"}):");
            string street = ConsoleHelper.ReadString("Podaj:", existingAddress == null || string.IsNullOrEmpty(existingAddress.Street));

            Console.WriteLine($"Nr bud. ({existingAddress?.BuildingNumber ?? "brak"}):");
            string building = ConsoleHelper.ReadString("Podaj:", existingAddress == null || string.IsNullOrEmpty(existingAddress.BuildingNumber));

            Console.WriteLine($"Nr miesz. ({existingAddress?.ApartmentNumber ?? "brak"}):");
            string? apt = ConsoleHelper.ReadString("Podaj:", true);

            return new Address
            {
                Id = existingAddress?.Id ?? 0,
                Country = string.IsNullOrWhiteSpace(country) ? (existingAddress?.Country ?? "") : country,
                City = string.IsNullOrWhiteSpace(city) ? (existingAddress?.City ?? "") : city,
                Street = string.IsNullOrWhiteSpace(street) ? (existingAddress?.Street ?? "") : street,
                BuildingNumber = string.IsNullOrWhiteSpace(building) ? (existingAddress?.BuildingNumber ?? "") : building,
                ApartmentNumber = string.IsNullOrWhiteSpace(apt) ? (existingAddress?.ApartmentNumber) : apt
            };
        }

        private async Task EditClient(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' edit client.");
            ConsoleHelper.DisplayHeader("Edycja Klienta");
            await ListClients(user, false);
            int clientId = ConsoleHelper.ReadInt("\nID klienta (0=anuluj):", 0);
            if (clientId == 0) return;

            try
            {
                var client = await _dbService.GetClientByIdAsync(clientId);
                if (client == null)
                {
                    ConsoleHelper.WriteWarning($"Nie znaleziono ID: {clientId}");
                    return;
                }

                Console.WriteLine($"Edytujesz: {client.FirstName} {client.LastName}");
                string fname = ConsoleHelper.ReadString($"Imię:", true);
                string lname = ConsoleHelper.ReadString($"Nazwisko:", true);
                string email = ConsoleHelper.ReadString($"Email:", true);
                string phone = ConsoleHelper.ReadString($"Telefon:", true);
                string parcelLocker = ConsoleHelper.ReadString($"Paczkomat:", true);

                bool clientChanged = false;
                if (!string.IsNullOrWhiteSpace(fname) && !fname.Equals(client.FirstName)) { client.FirstName = fname; clientChanged = true; }
                if (!string.IsNullOrWhiteSpace(lname) && !lname.Equals(client.LastName)) { client.LastName = lname; clientChanged = true; }
                if (!string.IsNullOrWhiteSpace(email) && !email.Equals(client.Email)) { client.Email = email; clientChanged = true; }
                if (!string.IsNullOrWhiteSpace(phone) && !phone.Equals(client.PhoneNumber)) { client.PhoneNumber = phone; clientChanged = true; }
                string? newParcel = string.IsNullOrWhiteSpace(parcelLocker) ? null : parcelLocker;
                if (newParcel != client.ParcelLockerId) { client.ParcelLockerId = newParcel; clientChanged = true; }

                Console.WriteLine($"\nEdycja adresu (ID: {client.AddressId}):");
                var currentAddress = await _dbService.GetAddressByIdAsync(client.AddressId);
                bool addressChanged = false;
                // Osobna obsługa edycji powiązanego adresu.
                if (currentAddress != null)
                {
                    var updatedAddress = ReadAddressFromConsole(currentAddress);
                    if (updatedAddress.Country != currentAddress.Country ||
                        updatedAddress.City != currentAddress.City ||
                        updatedAddress.Street != currentAddress.Street ||
                        updatedAddress.BuildingNumber != currentAddress.BuildingNumber ||
                        updatedAddress.ApartmentNumber != currentAddress.ApartmentNumber)
                    {
                        updatedAddress.Id = client.AddressId;
                        await _dbService.UpdateAddressAsync(updatedAddress);
                        Console.WriteLine("Adres OK.");
                        addressChanged = true;
                    }
                    else
                    {
                        Console.WriteLine("Adres bez zmian.");
                    }
                }
                else
                {
                    ConsoleHelper.WriteWarning($"Brak adresu {client.AddressId}.");
                }

                if (clientChanged || addressChanged)
                {
                    await _dbService.UpdateClientAsync(client);
                    ConsoleHelper.WriteSuccess($"Klient ID {clientId} OK.");
                    await _logger.LogInfoAsync($"User '{user.Login}' updated client {clientId}.");
                }
                else
                {
                    Console.WriteLine("Brak zmian.");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error edit client {clientId} by '{user.Login}'.", ex);
            }
        }

        private async Task DeleteClient(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' delete client.");
            ConsoleHelper.DisplayHeader("Usuwanie Klienta");
            await ListClients(user, false);
            int clientId = ConsoleHelper.ReadInt("\nID klienta (0=anuluj):", 0);
            if (clientId == 0) return;

            try
            {
                var client = await _dbService.GetClientByIdAsync(clientId);
                if (client == null)
                {
                    ConsoleHelper.WriteWarning($"Nie znaleziono ID: {clientId}");
                    return;
                }

                if (ConsoleHelper.Confirm($"Usunąć '{client.FirstName} {client.LastName}'? Usunie zamówienia!"))
                {
                    await _dbService.DeleteClientAsync(clientId);
                    ConsoleHelper.WriteSuccess($"Klient ID {clientId} usunięty.");
                    await _logger.LogInfoAsync($"User '{user.Login}' deleted client {clientId}.");
                }
                else
                {
                    Console.WriteLine("Anulowano.");
                    await _logger.LogInfoAsync($"User '{user.Login}' cancelled delete client {clientId}.");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error delete client {clientId} by '{user.Login}'.", ex);
            }
        }

        private async Task ShowClientAddress(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' view client address.");
            ConsoleHelper.DisplayHeader("Adres Klienta");
            await ListClients(user, false);
            int clientId = ConsoleHelper.ReadInt("\nID klienta (0=anuluj):", 0);
            if (clientId == 0) return;

            try
            {
                var client = await _dbService.GetClientByIdAsync(clientId);
                if (client == null)
                {
                    ConsoleHelper.WriteWarning($"Nie znaleziono ID: {clientId}");
                    return;
                }

                var address = await _dbService.GetAddressByIdAsync(client.AddressId);
                if (address == null)
                {
                    ConsoleHelper.WriteWarning($"Brak adresu ID: {client.AddressId}.");
                    return;
                }

                Console.WriteLine($"\nAdres klienta {client.FirstName} {client.LastName}:");
                Console.WriteLine($" ID: {address.Id}");
                Console.WriteLine($" {address.Street} {address.BuildingNumber}" + (!string.IsNullOrEmpty(address.ApartmentNumber) ? $"/{address.ApartmentNumber}" : ""));
                Console.WriteLine($" {address.City}, {address.Country}");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error show address for client {clientId} by '{user.Login}'.", ex);
            }
        }

        private async Task ManageOrders(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: ManageOrders");
            bool stayInMenu = true;
            while (stayInMenu)
            {
                ConsoleHelper.ClearScreen(); ConsoleHelper.DisplayHeader("Zarządzanie Zamówieniami");
                Console.WriteLine("1. Pokaż wymagające uwagi (Nowe, W trakcie)");
                Console.WriteLine("2. Pokaż wszystkie");
                Console.WriteLine("3. Zmień status zamówienia");
                Console.WriteLine("4. Pokaż szczegóły zamówienia");
                Console.WriteLine("0. Wróć"); Console.WriteLine("---------------------------");
                int choice = ConsoleHelper.ReadInt("Wybierz:", 0, 4);
                bool actionPaused = (choice == 1 || choice == 2 || choice == 4);
                switch (choice)
                {
                    case 1: await ListOrdersByStatus(user, OrderStatus.Nowe, OrderStatus.W_trakcie); break;
                    case 2: await ShowAllOrders(user, true); break;
                    case 3: await ChangeOrderStatus(user); break;
                    case 4: await ShowSelectedOrderDetails(user); break;
                    case 0: stayInMenu = false; break;
                }
                if (stayInMenu && !actionPaused) { Console.WriteLine("\nNaciśnij..."); Console.ReadKey(); }
                else if (stayInMenu && actionPaused) { Console.WriteLine("\nNaciśnij by wrócić..."); Console.ReadKey(); }
            }
        }
        private async Task ListOrdersByStatus(User user, params OrderStatus[] statuses)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' list orders by status: {string.Join(",", statuses)}");
            ConsoleHelper.DisplayHeader($"Zamówienia: {string.Join(", ", statuses)}");
            try
            {
                var orders = await _dbService.GetOrdersByStatusAsync(statuses);
                if (!orders.Any()) { Console.WriteLine($"Brak zamówień."); return; }
                Console.WriteLine($"{"ID",-10} | {"Klient",-25} | {"Data",-20} | {"Status",-15}");
                Console.WriteLine(new string('-', 75));
                foreach (var order in orders)
                {
                    string clientName = $"ID: {order.ClientId?.ToString() ?? "Brak"}";
                    Console.WriteLine($"{order.Id,-10} | {clientName,-25} | {order.OrderDate,-20:yyyy-MM-dd HH:mm} | {order.Status,-15}");
                }
                Console.WriteLine(new string('-', 75));
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Błąd: {ex.Message}"); await _logger.LogErrorAsync($"Error list orders by status by '{user.Login}'.", ex); }
        }
        private async Task ChangeOrderStatus(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' change order status.");
            ConsoleHelper.DisplayHeader("Zmiana Statusu Zamówienia");
            await ListOrdersByStatus(user, OrderStatus.Nowe, OrderStatus.W_trakcie);
            int orderId = ConsoleHelper.ReadInt("\nPodaj ID zamówienia (0=anuluj):", 0); if (orderId == 0) return;
            try
            {
                Console.WriteLine($"\nZmiana statusu dla ID: {orderId}");
                var availableStatuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();
                for (int i = 0; i < availableStatuses.Count; i++) { Console.WriteLine($"{i + 1}. {availableStatuses[i]}"); }
                int statusChoice = ConsoleHelper.ReadInt("Nr statusu:", 1, availableStatuses.Count);
                OrderStatus newStatus = availableStatuses[statusChoice - 1];
                if (ConsoleHelper.Confirm($"Zmieniasz status na '{newStatus}'. OK?"))
                {
                    await _dbService.UpdateOrderStatusAsync(orderId, newStatus);
                    ConsoleHelper.WriteSuccess($"Status {orderId} zmieniony.");
                    await _logger.LogInfoAsync($"User '{user.Login}' updated order {orderId} to {newStatus}.");
                }
                else { Console.WriteLine("Anulowano."); }
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Błąd: {ex.Message}"); await _logger.LogErrorAsync($"Error change order status {orderId} by '{user.Login}'.", ex); }
        }
        private async Task ShowSelectedOrderDetails(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' view order details.");
            ConsoleHelper.DisplayHeader("Szczegóły Zamówienia");
            int orderId = ConsoleHelper.ReadInt("Podaj ID zamówienia:");
            await ShowOrderDetails(orderId);
        }

        private async Task ManageSuppliers(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: ManageSuppliers");
            bool stayInMenu = true;
            while (stayInMenu)
            {
                ConsoleHelper.ClearScreen(); ConsoleHelper.DisplayHeader("Zarządzanie Dostawcami");
                Console.WriteLine("1. Lista"); Console.WriteLine("2. Dodaj"); Console.WriteLine("3. Edytuj");
                Console.WriteLine("4. Usuń"); Console.WriteLine("5. Pokaż adres"); Console.WriteLine("0. Wróć");
                Console.WriteLine("-------------");
                int choice = ConsoleHelper.ReadInt("Wybierz:", 0, 5);
                bool actionPaused = (choice == 1 || choice == 5);
                switch (choice)
                {
                    case 1: await ListSuppliers(user, true); break;
                    case 2: await AddNewSupplier(user); break;
                    case 3: await EditSupplier(user); break;
                    case 4: await DeleteSupplier(user); break;
                    case 5: await ShowSupplierAddress(user); break;
                    case 0: stayInMenu = false; break;
                }
                if (stayInMenu && !actionPaused) { Console.WriteLine("\nNaciśnij..."); Console.ReadKey(); }
                else if (stayInMenu && actionPaused) { Console.WriteLine("\nNaciśnij by wrócić..."); Console.ReadKey(); }
            }
        }
        private async Task ListSuppliers(User user, bool pause = true)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' list suppliers.");
            ConsoleHelper.DisplayHeader("Lista Dostawców");
            try
            {
                var suppliers = await _dbService.GetAllSuppliersAsync();
                if (!suppliers.Any()) { Console.WriteLine("Brak dostawców."); return; }
                Console.WriteLine($"{"ID",-5} | {"Nazwa",-45} | {"Kontakt",-30} | AdrID");
                Console.WriteLine(new string('-', 75));
                suppliers.ForEach(s => Console.WriteLine($"{s.Id,-5} | {s.Name,-45} | {s.ContactInfo ?? "Brak",-30} | {s.AddressId?.ToString() ?? "Brak"}"));
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Błąd: {ex.Message}"); await _logger.LogErrorAsync($"Error list suppliers by '{user.Login}'.", ex); }
        }
        private async Task AddNewSupplier(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' add supplier.");
            ConsoleHelper.DisplayHeader("Dodawanie Dostawcy");
            try
            {
                int? addressId = null;
                if (ConsoleHelper.Confirm("Dodać adres?"))
                {
                    var newAddress = ReadAddressFromConsole();
                    addressId = await _dbService.AddAddressAsync(newAddress);
                    ConsoleHelper.WriteSuccess($"Adres ID: {addressId} dodany.");
                }
                string name = ConsoleHelper.ReadString("Nazwa:");
                string? contact = ConsoleHelper.ReadString("Kontakt:", true);
                var newSupplier = new Supplier
                {
                    Name = name,
                    ContactInfo = string.IsNullOrWhiteSpace(contact) ? null : contact,
                    AddressId = addressId
                };
                await _dbService.AddSupplierAsync(newSupplier);
                ConsoleHelper.WriteSuccess($"Dostawca '{name}' dodany.");
                await _logger.LogInfoAsync($"User '{user.Login}' added supplier '{name}'.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error add supplier by '{user.Login}'.", ex);
            }
        }

        private async Task EditSupplier(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' edit supplier.");
            ConsoleHelper.DisplayHeader("Edycja Dostawcy");
            await ListSuppliers(user, false);
            int supplierId = ConsoleHelper.ReadInt("\nID dostawcy (0=anuluj):", 0);
            if (supplierId == 0) return;

            try
            {
                var supplier = await _dbService.GetSupplierByIdAsync(supplierId);
                if (supplier == null)
                {
                    ConsoleHelper.WriteWarning($"Nie znaleziono ID: {supplierId}");
                    return;
                }

                Console.WriteLine($"Edytujesz: {supplier.Name}");
                string name = ConsoleHelper.ReadString($"Nazwa:", true);
                string? contact = ConsoleHelper.ReadString($"Kontakt:", true);
                bool changed = false;

                if (!string.IsNullOrWhiteSpace(name) && !name.Equals(supplier.Name))
                {
                    supplier.Name = name;
                    changed = true;
                }
                if (contact != null)
                {
                    string? newContact = string.IsNullOrWhiteSpace(contact) ? null : contact;
                    if (newContact != supplier.ContactInfo)
                    {
                        supplier.ContactInfo = newContact;
                        changed = true;
                    }
                }

                if (supplier.AddressId.HasValue)
                {
                    if (ConsoleHelper.Confirm($"Ma adres ID: {supplier.AddressId}. Edytować?"))
                    {
                        var addr = await _dbService.GetAddressByIdAsync(supplier.AddressId.Value);
                        if (addr != null)
                        {
                            var updatedAddr = ReadAddressFromConsole(addr);
                            if (updatedAddr.Country != addr.Country ||
                                updatedAddr.City != addr.City ||
                                updatedAddr.Street != addr.Street ||
                                updatedAddr.BuildingNumber != addr.BuildingNumber ||
                                updatedAddr.ApartmentNumber != addr.ApartmentNumber)
                            {
                                updatedAddr.Id = addr.Id;
                                await _dbService.UpdateAddressAsync(updatedAddr);
                                Console.WriteLine("Adres OK.");
                                changed = true;
                            }
                            else
                            {
                                Console.WriteLine("Adres bez zmian.");
                            }
                        }
                        else
                        {
                            ConsoleHelper.WriteWarning($"Brak adresu {supplier.AddressId}.");
                        }
                    }
                }
                else
                {
                    if (ConsoleHelper.Confirm("Brak adresu. Dodać?"))
                    {
                        var newAddr = ReadAddressFromConsole();
                        supplier.AddressId = await _dbService.AddAddressAsync(newAddr);
                        Console.WriteLine($"Adres ID: {supplier.AddressId} dodany.");
                        changed = true;
                    }
                }

                if (changed)
                {
                    await _dbService.UpdateSupplierAsync(supplier);
                    ConsoleHelper.WriteSuccess($"Dostawca ID {supplierId} OK.");
                    await _logger.LogInfoAsync($"User '{user.Login}' updated supplier {supplierId}.");
                }
                else
                {
                    Console.WriteLine("Brak zmian.");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error edit supplier {supplierId} by '{user.Login}'.", ex);
            }
        }

        private async Task DeleteSupplier(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' delete supplier.");
            ConsoleHelper.DisplayHeader("Usuwanie Dostawcy");
            await ListSuppliers(user, false);
            int supplierId = ConsoleHelper.ReadInt("\nID dostawcy (0=anuluj):", 0);
            if (supplierId == 0) return;

            try
            {
                var supplier = await _dbService.GetSupplierByIdAsync(supplierId);
                if (supplier == null)
                {
                    ConsoleHelper.WriteWarning($"Nie znaleziono ID: {supplierId}");
                    return;
                }

                if (ConsoleHelper.Confirm($"Usunąć '{supplier.Name}'? Usunie powiązania!"))
                {
                    await _dbService.DeleteSupplierAsync(supplierId);
                    ConsoleHelper.WriteSuccess($"Dostawca ID {supplierId} usunięty.");
                    await _logger.LogInfoAsync($"User '{user.Login}' deleted supplier {supplierId}.");
                }
                else
                {
                    Console.WriteLine("Anulowano.");
                    await _logger.LogInfoAsync($"User '{user.Login}' cancelled delete supplier {supplierId}.");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error delete supplier {supplierId} by '{user.Login}'.", ex);
            }
        }

        private async Task ShowSupplierAddress(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' view supplier address.");
            ConsoleHelper.DisplayHeader("Adres Dostawcy");
            await ListSuppliers(user, false);
            int supplierId = ConsoleHelper.ReadInt("\nID dostawcy (0=anuluj):", 0);
            if (supplierId == 0) return;

            try
            {
                var supplier = await _dbService.GetSupplierByIdAsync(supplierId);
                if (supplier == null)
                {
                    ConsoleHelper.WriteWarning($"Nie znaleziono ID: {supplierId}");
                    return;
                }
                if (!supplier.AddressId.HasValue)
                {
                    ConsoleHelper.WriteWarning($"Dostawca '{supplier.Name}' nie ma adresu.");
                    return;
                }

                var address = await _dbService.GetAddressByIdAsync(supplier.AddressId.Value);
                if (address == null)
                {
                    ConsoleHelper.WriteWarning($"Brak adresu ID: {supplier.AddressId}.");
                    return;
                }

                Console.WriteLine($"\nAdres dostawcy {supplier.Name}:");
                Console.WriteLine($" ID: {address.Id}");
                Console.WriteLine($" {address.Street} {address.BuildingNumber}" + (!string.IsNullOrEmpty(address.ApartmentNumber) ? $"/{address.ApartmentNumber}" : ""));
                Console.WriteLine($" {address.City}, {address.Country}");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error show address for supplier {supplierId} by '{user.Login}'.", ex);
            }
        }

        private async Task ManageDeliveryLinks(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: ManageDeliveryLinks");
            bool stayInMenu = true;
            while (stayInMenu)
            {
                ConsoleHelper.ClearScreen(); ConsoleHelper.DisplayHeader("Zarządzanie Powiązaniami");
                Console.WriteLine("1. Lista"); Console.WriteLine("2. Dodaj"); Console.WriteLine("3. Usuń");
                Console.WriteLine("0. Wróć"); Console.WriteLine("-------------");
                int choice = ConsoleHelper.ReadInt("Wybierz:", 0, 3);
                bool actionPaused = (choice == 1);
                switch (choice)
                {
                    case 1: await ListDeliveryLinks(user, true); break;
                    case 2: await AddNewDeliveryLink(user); break;
                    case 3: await RemoveDeliveryLink(user); break;
                    case 0: stayInMenu = false; break;
                }
                if (stayInMenu && !actionPaused) { Console.WriteLine("\nNaciśnij..."); Console.ReadKey(); }
                else if (stayInMenu && actionPaused) { Console.WriteLine("\nNaciśnij by wrócić..."); Console.ReadKey(); }
            }
        }
        private async Task ListDeliveryLinks(User user, bool pause = true)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' list delivery links.");
            ConsoleHelper.DisplayHeader("Lista Powiązań");
            try
            {
                var links = await _dbService.GetAllDeliveryLinksAsync();
                if (!links.Any()) { Console.WriteLine("Brak powiązań."); return; }
                Console.WriteLine($"{"ID Dost.",-5} | {"Dostawca",-45} | {"ID Prod.",-6} | {"Produkt"}");
                Console.WriteLine(new string('-', 90));
                links.ForEach(link => Console.WriteLine($"{link.Supplier.Id,-5} | {link.Supplier.Name,-45} | {link.Product.Id,-6} | {link.Product.Name}"));
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Błąd: {ex.Message}"); await _logger.LogErrorAsync($"Error list links by '{user.Login}'.", ex); }
        }
        private async Task AddNewDeliveryLink(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' add delivery link.");
            ConsoleHelper.DisplayHeader("Dodawanie Powiązania");
            try
            {
                await ListSuppliers(user, false);
                int supplierId = ConsoleHelper.ReadInt("\nID Dostawcy:");
                if (await _dbService.GetSupplierByIdAsync(supplierId) == null) { ConsoleHelper.WriteWarning("Złe ID dostawcy."); return; }
                await ShowAllProducts(user);
                int productId = ConsoleHelper.ReadInt("\nID Produktu:");
                if (await _dbService.GetProductByIdAsync(productId) == null) { ConsoleHelper.WriteWarning("Złe ID produktu."); return; }
                await _dbService.AddDeliveryLinkAsync(supplierId, productId);
                ConsoleHelper.WriteSuccess($"Powiązanie {supplierId} <-> {productId} dodane.");
                await _logger.LogInfoAsync($"User '{user.Login}' added link S:{supplierId}-P:{productId}.");
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Błąd: {ex.Message}"); await _logger.LogErrorAsync($"Error add link by '{user.Login}'.", ex); }
        }
        private async Task RemoveDeliveryLink(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' remove delivery link.");
            ConsoleHelper.DisplayHeader("Usuwanie Powiązania");
            await ListDeliveryLinks(user, false);
            try
            {
                int supplierId = ConsoleHelper.ReadInt("\nID Dostawcy:"); int productId = ConsoleHelper.ReadInt("ID Produktu:");
                if (ConsoleHelper.Confirm($"Usunąć powiązanie {supplierId} <-> {productId}?"))
                {
                    await _dbService.RemoveDeliveryLinkAsync(supplierId, productId);
                    ConsoleHelper.WriteSuccess("Usunięto.");
                    await _logger.LogInfoAsync($"User '{user.Login}' removed link S:{supplierId}-P:{productId}.");
                }
                else { Console.WriteLine("Anulowano."); }
            }
            catch (Exception ex) { ConsoleHelper.WriteError($"Błąd: {ex.Message}"); await _logger.LogErrorAsync($"Error remove link by '{user.Login}'.", ex); }
        }

        private async Task ShowAllOrders(User user, bool pause = true)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: ShowAllOrders");
            if (pause) ConsoleHelper.ClearScreen();
            ConsoleHelper.DisplayHeader("Wszystkie Zamówienia");
            bool detailsShown = false;
            try
            {
                var allOrders = await _dbService.GetAllOrdersAsync();
                if (!allOrders.Any())
                {
                    Console.WriteLine("Brak zamówień w systemie.");
                    return;
                }
                Console.WriteLine($"{"ID Zam.",-10} | {"Klient",-25} | {"Data",-20} | {"Status",-15}");
                Console.WriteLine(new string('-', 75));
                foreach (var order in allOrders)
                {
                    string clientName = $"ID: {order.ClientId?.ToString() ?? "Brak"}";
                    Console.WriteLine($"{order.Id,-10} | {clientName,-25} | {order.OrderDate,-20:yyyy-MM-dd HH:mm} | {order.Status,-15}");
                }
                Console.WriteLine(new string('-', 75));
                int orderIdToShow = ConsoleHelper.ReadInt("Podaj ID zamówienia do szczegółów (lub 0):", 0);
                if (orderIdToShow > 0)
                {
                    if (allOrders.Any(o => o.Id == orderIdToShow))
                    {
                        await ShowOrderDetails(orderIdToShow);
                        detailsShown = true;
                    }
                    else
                    {
                        ConsoleHelper.WriteWarning("Nie ma zamówienia o podanym ID na liście.");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd podczas pobierania zamówień: {ex.Message}");
                await _logger.LogErrorAsync($"Error showing all orders by user '{user.Login}'.", ex);
            }
            if (pause && detailsShown)
            {
                Console.WriteLine("\nNaciśnij by wrócić...");
                Console.ReadKey();
            }
        }

        private async Task ManageBillTypes(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: ManageBillTypes");
            bool stayInMenu = true;
            while (stayInMenu)
            {
                ConsoleHelper.ClearScreen();
                ConsoleHelper.DisplayHeader("Zarządzanie Typami Rachunków");
                Console.WriteLine("1. Lista");
                Console.WriteLine("2. Dodaj");
                Console.WriteLine("3. Edytuj");
                Console.WriteLine("4. Usuń");
                Console.WriteLine("0. Wróć");
                Console.WriteLine("-------------");
                int choice = ConsoleHelper.ReadInt("Wybierz:", 0, 4);
                bool actionPaused = (choice == 1);
                switch (choice)
                {
                    case 1:
                        await ListBillTypes(user, true);
                        break;
                    case 2:
                        await AddNewBillType(user);
                        break;
                    case 3:
                        await EditBillType(user);
                        break;
                    case 4:
                        await DeleteBillType(user);
                        break;
                    case 0:
                        stayInMenu = false;
                        break;
                }
                if (stayInMenu && !actionPaused)
                {
                    Console.WriteLine("\nNaciśnij...");
                    Console.ReadKey();
                }
                else if (stayInMenu && actionPaused)
                {
                    continue;
                }
            }
        }

        private async Task ListBillTypes(User user, bool pause = true)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' list bill types.");
            ConsoleHelper.DisplayHeader("Lista Typów Rachunków");
            try
            {
                var types = await _dbService.GetAllBillTypesAsync();
                if (!types.Any())
                {
                    Console.WriteLine("Brak typów.");
                }
                else
                {
                    Console.WriteLine($"{"ID",-5} | {"Typ",-30} | {"Opis"}");
                    Console.WriteLine(new string('-', 70));
                    types.ForEach(t => Console.WriteLine($"{t.Id,-5} | {t.TypeName,-30} | {t.Description ?? "Brak"}"));
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error list bill types by '{user.Login}'.", ex);
            }
            if (pause)
            {
                Console.WriteLine("\nNaciśnij by wrócić...");
                Console.ReadKey();
            }
        }

        private async Task AddNewBillType(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' add bill type.");
            ConsoleHelper.DisplayHeader("Dodawanie Typu");
            try
            {
                string name = ConsoleHelper.ReadString("Nazwa:");
                if (string.IsNullOrWhiteSpace(name))
                {
                    ConsoleHelper.WriteWarning("Nazwa typu nie może być pusta.");
                    return;
                }
                var existingType = await _dbService.GetBillTypeByNameAsync(name);
                if (existingType != null)
                {
                    ConsoleHelper.WriteError($"Typ '{name}' już istnieje.");
                    return;
                }
                string? desc = ConsoleHelper.ReadString("Opis:", true);
                var newType = new BillType { TypeName = name, Description = string.IsNullOrWhiteSpace(desc) ? null : desc };
                await _dbService.AddBillTypeAsync(newType);
                ConsoleHelper.WriteSuccess($"Typ '{name}' dodany.");
                await _logger.LogInfoAsync($"User '{user.Login}' added bill type '{name}'.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error add bill type by '{user.Login}'.", ex);
            }
        }

        private async Task EditBillType(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' edit bill type.");
            ConsoleHelper.DisplayHeader("Edycja Typu");
            await ListBillTypes(user, false);
            int typeId = ConsoleHelper.ReadInt("\nID typu (0=anuluj):", 0);
            if (typeId == 0) return;
            try
            {
                var type = await _dbService.GetBillTypeByIdAsync(typeId);
                if (type == null)
                {
                    ConsoleHelper.WriteWarning($"Nie znaleziono ID: {typeId}");
                    return;
                }
                Console.WriteLine($"Edytujesz: {type.TypeName}");
                string nameInput = ConsoleHelper.ReadString($"Nazwa [{type.TypeName}]:", true);
                string? descInput = ConsoleHelper.ReadString($"Opis [{type.Description ?? "Brak"}]:", true);

                string originalName = type.TypeName; 
                string? originalDesc = type.Description;

                bool nameChanged = false;
                bool descChanged = false;

                if (!string.IsNullOrWhiteSpace(nameInput) && !nameInput.Equals(originalName, StringComparison.OrdinalIgnoreCase))
                {
                    var existing = await _dbService.GetBillTypeByNameAsync(nameInput);
                    if (existing != null && existing.Id != typeId)
                    {
                        ConsoleHelper.WriteError($"Typ '{nameInput}' już istnieje.");
                        return;
                    }
                    type.TypeName = nameInput;
                    nameChanged = true;
                }

                if (descInput != null) // Jeśli użytkownik coś wpisał (nawet spacje)
                {
                    string? newDesc = string.IsNullOrWhiteSpace(descInput) ? null : descInput.Trim();
                    if (newDesc != originalDesc) // Porównaj z oryginalnym opisem
                    {
                        type.Description = newDesc;
                        descChanged = true;
                    }
                }

                if (nameChanged || descChanged)
                {
                    await _dbService.UpdateBillTypeAsync(type);
                    ConsoleHelper.WriteSuccess($"Typ ID {typeId} zaktualizowany OK.");
                    await _logger.LogInfoAsync($"User '{user.Login}' updated bill type {typeId}. Name changed: {nameChanged}, Desc changed: {descChanged}.");
                }
                else
                {
                    Console.WriteLine("Brak zmian.");
                }
            }
            // Obsługa błędu MySQL (1062) - próba dodania typu o istniejącej nazwie (naruszenie UNIQUE constraint).
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ConsoleHelper.WriteError($"Błąd DB: Typ o tej nazwie już istnieje.");
                await _logger.LogErrorAsync($"Error edit bill type {typeId}: Duplicate name attempt.", ex);
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error edit bill type {typeId} by '{user.Login}'.", ex);
            }
        }


        private async Task DeleteBillType(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' delete bill type.");
            ConsoleHelper.DisplayHeader("Usuwanie Typu");
            await ListBillTypes(user, false);
            int typeId = ConsoleHelper.ReadInt("\nID typu (0=anuluj):", 0);
            if (typeId == 0) return;
            try
            {
                var type = await _dbService.GetBillTypeByIdAsync(typeId);
                if (type == null)
                {
                    ConsoleHelper.WriteWarning($"Nie znaleziono ID: {typeId}");
                    return;
                }
                if (ConsoleHelper.Confirm($"Usunąć typ '{type.TypeName}'?"))
                {
                    await _dbService.DeleteBillTypeAsync(typeId);
                    ConsoleHelper.WriteSuccess($"Typ ID {typeId} usunięty.");
                    await _logger.LogInfoAsync($"User '{user.Login}' deleted bill type {typeId}.");
                }
                else
                {
                    Console.WriteLine("Anulowano.");
                    await _logger.LogInfoAsync($"User '{user.Login}' cancelled delete bill type {typeId}.");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error delete bill type {typeId} by '{user.Login}'.", ex);
            }
        }

        private async Task ManageBills(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' executing: ManageBills");
            bool stayInMenu = true;
            while (stayInMenu)
            {
                ConsoleHelper.ClearScreen();
                ConsoleHelper.DisplayHeader("Zarządzanie Rachunkami");
                Console.WriteLine("1. Lista");
                Console.WriteLine("2. Dodaj");
                Console.WriteLine("3. Edytuj");
                Console.WriteLine("0. Wróć");
                Console.WriteLine("-------------");
                int choice = ConsoleHelper.ReadInt("Wybierz:", 0, 3);
                bool actionPaused = (choice == 1);
                switch (choice)
                {
                    case 1:
                        await ListBills(user, true);
                        break;
                    case 2:
                        await AddNewBill(user);
                        break;
                    case 3:
                        await EditBill(user);
                        break;
                    case 0:
                        stayInMenu = false;
                        break;
                }
                if (stayInMenu && !actionPaused)
                {
                    Console.WriteLine("\nNaciśnij...");
                    Console.ReadKey();
                }
                else if (stayInMenu && actionPaused)
                {
                    Console.WriteLine("\nNaciśnij by wrócić...");
                    Console.ReadKey();
                }
            }
        }

        private async Task ListBills(User user, bool pause = true)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' list bills.");
            ConsoleHelper.DisplayHeader("Lista Rachunków");
            try
            {
                var bills = await _dbService.GetAllBillsAsync();
                if (!bills.Any())
                {
                    Console.WriteLine("Brak rachunków.");
                    return;
                }
                Console.WriteLine($"{"ID",-5} | {"Data",-12} | {"Typ ID",-8} | {"Kwota",-15} | {"Nazwa Typu"}");
                Console.WriteLine(new string('-', 75));
                var billTypes = (await _dbService.GetAllBillTypesAsync()).ToDictionary(t => t.Id);
                foreach (var bill in bills)
                {
                    string typeName = billTypes.TryGetValue(bill.BillTypeId, out var type) ? type.TypeName : $"Brak(ID:{bill.BillTypeId})";
                    Console.WriteLine($"{bill.Id,-5} | {bill.BillDate,-12:yyyy-MM-dd} | {bill.BillTypeId,-8} | {bill.Amount,-15:C} | {typeName}");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error list bills by '{user.Login}'.", ex);
            }
        }

        private async Task AddNewBill(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' add bill.");
            ConsoleHelper.DisplayHeader("Dodawanie Rachunku");
            try
            {
                await ListBillTypes(user, false);
                var billTypes = await _dbService.GetAllBillTypesAsync();
                if (!billTypes.Any())
                {
                    ConsoleHelper.WriteWarning("Brak typów.");
                    return;
                }
                int typeId = ConsoleHelper.ReadInt("\nWybierz ID Typu:");
                if (!billTypes.Any(t => t.Id == typeId))
                {
                    ConsoleHelper.WriteError("Złe ID.");
                    return;
                }
                DateOnly billDate;
                string dateStr = ConsoleHelper.ReadString("Data (YYYY-MM-DD/Enter=dziś):", true);
                if (string.IsNullOrWhiteSpace(dateStr))
                {
                    billDate = DateOnly.FromDateTime(DateTime.Now);
                }
                else if (!DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out billDate))
                {
                    ConsoleHelper.WriteError("Zły format.");
                    return;
                }
                decimal amount = ConsoleHelper.ReadDecimal("Kwota:");
                var newBill = new Bill { BillDate = billDate, BillTypeId = typeId, Amount = amount };
                await _dbService.AddBillAsync(newBill);
                ConsoleHelper.WriteSuccess($"Rachunek dodany.");
                await _logger.LogInfoAsync($"User '{user.Login}' added bill.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error add bill by '{user.Login}'.", ex);
            }
        }

        private async Task EditBill(User user)
        {
            await _logger.LogInfoAsync($"User '{user.Login}' edit bill.");
            ConsoleHelper.DisplayHeader("Edycja Rachunku");
            await ListBills(user, false);
            int billId = ConsoleHelper.ReadInt("\nID rachunku (0=anuluj):", 0);
            if (billId == 0) return;
            try
            {
                var bill = await _dbService.GetBillByIdAsync(billId);
                if (bill == null)
                {
                    ConsoleHelper.WriteWarning($"Nie znaleziono ID: {billId}");
                    return;
                }
                var currentType = await _dbService.GetBillTypeByIdAsync(bill.BillTypeId);
                string currentTypeName = currentType?.TypeName ?? $"ID:{bill.BillTypeId}";
                Console.WriteLine($"Edytujesz ID: {bill.Id}, Data: {bill.BillDate:yyyy-MM-dd}, Typ: {currentTypeName}, Kwota: {bill.Amount:C}");
                Console.WriteLine("---");
                DateOnly newDate = bill.BillDate;
                string dateStr = ConsoleHelper.ReadString("Nowa data:", true);
                if (!string.IsNullOrWhiteSpace(dateStr))
                {
                    if (!DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out newDate))
                    {
                        ConsoleHelper.WriteError("Zły format daty."); newDate = bill.BillDate;
                    }
                }
                int newTypeId = bill.BillTypeId;
                await ListBillTypes(user, false);
                Console.WriteLine($"Obecny typ ID: {bill.BillTypeId}");
                string typeStr = ConsoleHelper.ReadString("Nowe ID Typu:", true);
                if (!string.IsNullOrWhiteSpace(typeStr))
                {
                    if (int.TryParse(typeStr, out int parsedId))
                    {
                        if (await _dbService.GetBillTypeByIdAsync(parsedId) != null)
                        {
                            newTypeId = parsedId;
                        }
                        else
                        {
                            ConsoleHelper.WriteWarning("Złe ID typu.");
                            newTypeId = bill.BillTypeId;
                        }
                    }
                    else
                    {
                        ConsoleHelper.WriteWarning("Złe ID typu.");
                        newTypeId = bill.BillTypeId; 
                    }
                }
                decimal newAmount = bill.Amount;
                string amountStr = ConsoleHelper.ReadString("Nowa kwota:", true);
                if (!string.IsNullOrWhiteSpace(amountStr))
                {
                    if (decimal.TryParse(amountStr.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedAmount))
                    {
                        newAmount = parsedAmount;
                    }
                    else
                    {
                        ConsoleHelper.WriteWarning("Zła kwota.");
                        newAmount = bill.Amount; 
                    }
                }
                bool changed = (newDate != bill.BillDate) || (newTypeId != bill.BillTypeId) || (newAmount != bill.Amount);
                if (changed)
                {
                    bill.BillDate = newDate;
                    bill.BillTypeId = newTypeId;
                    bill.Amount = newAmount;
                    await _dbService.UpdateBillAsync(bill);
                    ConsoleHelper.WriteSuccess($"Rachunek ID {billId} OK.");
                    await _logger.LogInfoAsync($"User '{user.Login}' updated bill {billId}.");
                }
                else
                {
                    Console.WriteLine("Brak zmian.");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
                await _logger.LogErrorAsync($"Error edit bill {billId} by '{user.Login}'.", ex);
            }
        }

        private async Task ShowOrderDetails(int orderId)
        {
            Console.WriteLine($"\n--- Szczegóły zamówienia nr {orderId} ---");
            try
            {
                var details = await _dbService.GetOrderDetailsAsync(orderId);
                if (!details.Any())
                {
                    Console.WriteLine("Brak szczegółów.");
                    return;
                }
                decimal totalAmount = 0;
                Console.WriteLine($"{"ID Prod.",-10} | {"Nazwa",-45} | {"Ilość",-6} | {"Cena",-12} | {"Suma"}");
                Console.WriteLine(new string('-', 95));
                var productIds = details.Select(d => d.ProductId).Distinct().ToList();
                var products = new Dictionary<int, Product?>();
                foreach (var pid in productIds)
                {
                    products.Add(pid, await _dbService.GetProductByIdAsync(pid));
                }
                foreach (var detail in details)
                {
                    var product = products.GetValueOrDefault(detail.ProductId);
                    string productName = product?.Name ?? $"ID:{detail.ProductId}";
                    decimal price = product?.Price ?? 0;
                    decimal lineTotal = price * detail.Quantity;
                    totalAmount += lineTotal;
                    Console.WriteLine($"{detail.ProductId,-10} | {productName,-45} | {detail.Quantity,-6} | {price,-12:C} | {lineTotal:C}");
                }
                Console.WriteLine(new string('-', 95));
                Console.WriteLine($"SUMA: {totalAmount:C}");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Błąd: {ex.Message}");
            }
        }

    }
}