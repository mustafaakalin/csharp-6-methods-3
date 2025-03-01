using System;
using System.Collections.Generic;
using System.Linq;

namespace ECommerceSystem
{
    // Ürün sınıfı
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public ProductCategory Category { get; set; }
    }

    // Ürün kategorileri
    public enum ProductCategory
    {
        Kitchen,
        Furniture
    }

    // İndirim türleri
    public enum DiscountType
    {
        Fixed,
        Percentage
    }

    // Kupon/Kampanya sınıfı
    public class Discount
    {
        public required string Code { get; set; }
        public DiscountType Type { get; set; }
        public decimal Value { get; set; }
    }

    // Sepet item sınıfı
    public class CartItem
    {
        public required Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal GetTotalPrice() => Product.Price * Quantity;
    }

    // Sepet sınıfı
    public class ShoppingCart
    {
        private List<CartItem> items = new List<CartItem>();
        private const decimal VAT_RATE = 0.20m; // %20 KDV

        public void AddItem(Product product, int quantity)
        {
            var existingItem = items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                items.Add(new CartItem { Product = product, Quantity = quantity });
            }
        }

        public void RemoveItem(int productId)
        {
            var item = items.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                items.Remove(item);
            }
        }

        public List<CartItem> GetItems() => items;

        public decimal CalculateSubtotal()
        {
            return items.Sum(item => item.GetTotalPrice());
        }

        public decimal CalculateVAT()
        {
            return CalculateSubtotal() * VAT_RATE;
        }

        public decimal ApplyDiscount(Discount discount)
        {
            var subtotal = CalculateSubtotal();
            if (discount.Type == DiscountType.Fixed)
            {
                return Math.Min(subtotal, discount.Value);
            }
            return subtotal * (discount.Value / 100);
        }

        public void GenerateInvoice(Discount? appliedDiscount = null)
        {
            Console.WriteLine("\n========= FATURA =========");
            Console.WriteLine("Ürünler:");
            foreach (var item in items)
            {
                Console.WriteLine($"{item.Product.Name} x{item.Quantity} = {item.GetTotalPrice():C2}");
            }
            
            var subtotal = CalculateSubtotal();
            Console.WriteLine($"\nAra Toplam: {subtotal:C2}");
            
            if (appliedDiscount != null)
            {
                var discountAmount = ApplyDiscount(appliedDiscount);
                Console.WriteLine($"İndirim ({appliedDiscount.Code}): -{discountAmount:C2}");
                subtotal -= discountAmount;
            }
            
            var vat = CalculateVAT();
            Console.WriteLine($"KDV (%20): {vat:C2}");
            Console.WriteLine($"Genel Toplam: {(subtotal + vat):C2}");
            Console.WriteLine("=========================");
        }
    }

    class Program
    {
        static List<Product> products = new List<Product>
        {
            new Product { Id = 1, Name = "Tencere Seti", Price = 1500m, Category = ProductCategory.Kitchen },
            new Product { Id = 2, Name = "Kahve Makinesi", Price = 2500m, Category = ProductCategory.Kitchen },
            new Product { Id = 3, Name = "L Koltuk", Price = 15000m, Category = ProductCategory.Furniture },
            new Product { Id = 4, Name = "Yemek Masası", Price = 8000m, Category = ProductCategory.Furniture }
        };

        static List<Discount> discounts = new List<Discount>
        {
            new Discount { Code = "YILBASI", Type = DiscountType.Percentage, Value = 15 },
            new Discount { Code = "HOSGELDIN", Type = DiscountType.Fixed, Value = 500 }
        };

        static void DisplayMenu()
        {
            Console.WriteLine("\n=== E-TİCARET SİSTEMİ ===");
            Console.WriteLine("1. Ürünleri Listele");
            Console.WriteLine("2. Kategoriye Göre Ürünleri Listele");
            Console.WriteLine("3. Sepete Ürün Ekle");
            Console.WriteLine("4. Sepeti Görüntüle");
            Console.WriteLine("5. Sepetten Ürün Çıkar");
            Console.WriteLine("6. Alışverişi Tamamla");
            Console.WriteLine("0. Çıkış");
            Console.Write("Seçiminiz: ");
        }

        static void ListProducts(List<Product> productsToList)
        {
            Console.WriteLine("\n=== ÜRÜNLER ===");
            foreach (var product in productsToList)
            {
                Console.WriteLine($"ID: {product.Id}, {product.Name} - {product.Price:C2} ({product.Category})");
            }
        }

        static void ListCategories()
        {
            Console.WriteLine("\n=== KATEGORİLER ===");
            foreach (ProductCategory category in Enum.GetValues(typeof(ProductCategory)))
            {
                Console.WriteLine($"{(int)category + 1}. {category}");
            }
        }

        static void Main(string[] args)
        {
            var cart = new ShoppingCart();
            bool running = true;

            while (running)
            {
                DisplayMenu();
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ListProducts(products);
                        break;

                    case "2":
                        ListCategories();
                        Console.Write("Kategori seçin (1-2): ");
                        if (int.TryParse(Console.ReadLine(), out int categoryChoice) && categoryChoice >= 1 && categoryChoice <= 2)
                        {
                            var category = (ProductCategory)(categoryChoice - 1);
                            var filteredProducts = products.Where(p => p.Category == category).ToList();
                            ListProducts(filteredProducts);
                        }
                        break;

                    case "3":
                        ListProducts(products);
                        Console.Write("Eklemek istediğiniz ürünün ID'sini girin: ");
                        if (int.TryParse(Console.ReadLine(), out int productId))
                        {
                            var product = products.FirstOrDefault(p => p.Id == productId);
                            if (product != null)
                            {
                                Console.Write("Miktar: ");
                                if (int.TryParse(Console.ReadLine(), out int quantity) && quantity > 0)
                                {
                                    cart.AddItem(product, quantity);
                                    Console.WriteLine("Ürün sepete eklendi!");
                                }
                            }
                        }
                        break;

                    case "4":
                        if (cart.GetItems().Any())
                        {
                            cart.GenerateInvoice();
                        }
                        else
                        {
                            Console.WriteLine("Sepetiniz boş!");
                        }
                        break;

                    case "5":
                        if (cart.GetItems().Any())
                        {
                            Console.WriteLine("\n=== SEPETİNİZ ===");
                            foreach (var item in cart.GetItems())
                            {
                                Console.WriteLine($"ID: {item.Product.Id}, {item.Product.Name} x{item.Quantity}");
                            }
                            Console.Write("Çıkarmak istediğiniz ürünün ID'sini girin: ");
                            if (int.TryParse(Console.ReadLine(), out int removeId))
                            {
                                cart.RemoveItem(removeId);
                                Console.WriteLine("Ürün sepetten çıkarıldı!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Sepetiniz boş!");
                        }
                        break;

                    case "6":
                        if (cart.GetItems().Any())
                        {
                            Console.Write("Kupon kodunuz var mı? (E/H): ");
                            if (Console.ReadLine()?.ToUpper() == "E")
                            {
                                Console.Write("Kupon kodunu girin: ");
                                string? couponCode = Console.ReadLine();
                                var discount = discounts.FirstOrDefault(d => d.Code.Equals(couponCode, StringComparison.OrdinalIgnoreCase));
                                cart.GenerateInvoice(discount);
                            }
                            else
                            {
                                cart.GenerateInvoice();
                            }
                            Console.WriteLine("Alışveriş tamamlandı! Teşekkür ederiz!");
                            running = false;
                        }
                        else
                        {
                            Console.WriteLine("Sepetiniz boş! Alışverişi tamamlayamazsınız.");
                        }
                        break;

                    case "0":
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Geçersiz seçim!");
                        break;
                }

                if (running)
                {
                    Console.WriteLine("\nDevam etmek için bir tuşa basın...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }
    }
}