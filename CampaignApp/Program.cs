// See https://aka.ms/new-console-template for more information

using System.Linq;

public class Program
{
    public static void DisplayStoreProducts(List<Item> products)
    {
        Console.WriteLine("Choose a product:");
        for (int i = 0; i < products.Count; i++)
        {
            Console.WriteLine($"{i+1}) {products[i].Name.PadRight(20, '.')}{products[i].Price}");
        }
        Console.Write("Type a number and enter: ");
    }

    public static bool TakeInput(List<Item> products, ShoppingCart cart)
    {
        string input = Console.ReadLine();
        if (int.TryParse(input, out int itemOrder))
        {
            if (0 < itemOrder && itemOrder <= products.Count())
            {
                var product = products[itemOrder - 1];
                cart.AddToCart(new Item(product.Name, product.EanCode, product.Price));
                return true;
            }
        }
        return false;
    }

    public static void DisplayShoppingCart(ShoppingCart cart)
    {
        var rows = cart.GetReceiptRows();

        rows.RemoveAll(row => row.GetType().Equals(typeof(Discount)));
        cart.CalculateCampaignPrice();


        Console.WriteLine();
        Console.WriteLine($" SHOPPING CART ".PadLeft(31,'-').PadRight(48, '-'));
        Console.WriteLine();
        Console.BackgroundColor = ConsoleColor.DarkGreen;
        var currentColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Product{"".PadRight(21, ' ')}EanCode{"".PadRight(14, ' ')}Price");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = currentColor;
        for (int i = 0; i < rows.Count(); i++)
        {
            var Id = rows[i].GetType().Equals(typeof(CartItem)) ? ((CartItem)rows[i]).Id.ToString() : "";
            Console.WriteLine($"{i + 1}) {rows[i].Name.PadRight(25, '.')}{rows[i].Info.PadRight(18, '.')}{rows[i].Price.ToString().PadLeft(5, '.')} --- {Id} ");
        }
        Console.WriteLine("".PadLeft(48, '-'));

        

        Console.WriteLine($"Total: {cart.GetTotalPrice().ToString().PadLeft(41, '-')}");
        Console.WriteLine("\n");
    }
    public static void Main(string [] args)
    {
        // Campaigns
        var comboCampaign = new ComboCampaign("Combo Campaign", "2 for 30", new string[] {
            "5000112637922",
            "5000112637939",
            "7310865004703",
            "7340005404261",
            "7310532109090",
            "7611612222105"}, 30);
        var volumeCampaign1 = new VolumeCampaign("Gilette Volume Campaign", "2 for 85", "8711000530085", 85, 2); // Gillete Foam Volume Campaign 
        var volumeCampaign2 = new VolumeCampaign("Coca Cola Volume Campaign", "2 for 20", "7310865004703", 20, 2); // Coca Cola Campaign
        var campaigns = new List<Campaign>() { comboCampaign, volumeCampaign1, volumeCampaign2 };

        // Store Items
        List<Item> products = new List<Item>();
        products.Add(new Item("Toothbrush 2p",  "5000112637939", 19.90m));  // Combo
        products.Add(new Item("Tooth paste",    "7340005404261", 19.90m));  // Combo
        products.Add(new Item("Protein bar",    "5000112637922", 24.95m));  // Combo
        products.Add(new Item("Chewing gum",    "7611612222105", 24.90m));  // Combo
        products.Add(new Item("Fanta Exotic",   "7310532109090", 17.95m));  // Combo
        products.Add(new Item("Coca Cola 50cl", "7310865004703", 17.95m));  // Combo + Volume
        products.Add(new Item("Gilette Foam",   "8711000530085", 65.95m));  // Volume


        // Create Shopping Cart
        var cart = new ShoppingCart(campaigns);

        // Shopping Loop
        bool isShopping = true;
        while(isShopping)
        {
            // Display Shopping Cart
            if (cart.HasItems())
            {
                DisplayShoppingCart(cart);
            }
            // Display Store Products
            DisplayStoreProducts(products);
            isShopping = TakeInput(products, cart);
            Console.Clear();

        }
    }
}

public class Campaign
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class ComboCampaign : Campaign
{
    public List<string> EanCodes { get; set; }

    public ComboCampaign(string name, string description, ICollection<string> eanCodes, int price)
    {
        Name = name;
        Description = description;
        EanCodes = new List<string>();
        EanCodes.AddRange(eanCodes);
        Price = price;
        Quantity = 2;
    }
}

public class VolumeCampaign : Campaign
{   
    public string EanCode { get; set; }

    public VolumeCampaign(string name, string description, string eanCode, decimal price, int quantity)
    {
        Name = name;
        Description = description;
        EanCode = eanCode;
        Price = price;
        Quantity = quantity;
    }
}
    
public class ShoppingCart
{
    List<ReceiptRow> ReceiptRows { get; set; }
    List<Campaign> Campaigns { get; set; }

    public ShoppingCart(ICollection<Campaign> campaigns)
    {
        Campaigns = new List<Campaign>();
        Campaigns.AddRange(campaigns);
        ReceiptRows = new List<ReceiptRow>();
    }

    public void AddToCart(Item item)
    {
        ReceiptRows.Add(new CartItem(GetCartItems().Count(), item));
    }

    public decimal GetTotalPrice()
    {
        return ReceiptRows.Sum(item => item.Price);
    }
    
    public bool HasItems()
    {
        return ReceiptRows.Any(row => row.GetType().Equals(typeof(CartItem)));
    }

    public List<CartItem> GetCartItems()
    {
        return ReceiptRows.Where(row => row.GetType().Equals(typeof(CartItem))).Select(item=>(CartItem)item).ToList();
    }

    public List<Discount> GetDiscounts()
    {
        return ReceiptRows.Where(row => row.GetType().Equals(typeof(Discount))).Select(item => (Discount)item).ToList();
    }

    public List<ReceiptRow> GetReceiptRows()
    {
        return ReceiptRows;
    }

    public void CalculateCampaignPrice(bool first = true)
    {
        var combos = new List<CartItem>();
        Campaigns.ForEach(campaign =>
        {
            var items = GetCartItems();
            if (campaign.GetType() == typeof(VolumeCampaign))
            {
                var volumes = new List<CartItem>();
                var volumeCampaign = (VolumeCampaign)campaign;
                for (int i = 0; i < items.Count; i++)
                {
                    if (volumeCampaign.EanCode.Contains(items[i].Info))
                    {
                        volumes.Add(items[i]);
                    }
                }
                // Make sure we meet the minimum requirement for the campaign
                while (volumes.Count >= volumeCampaign.Quantity)
                {
                    decimal discount = volumes[0].Price + volumes[1].Price - volumeCampaign.Price;
                    if (discount > 0)
                    {
                        discount *= -1;
                        // Reference to what products the discount refer to
                        var discountedItems = new List<CartItem>() { volumes[0], volumes[1] };
                        ReceiptRows.Add(new Discount(volumeCampaign, discount, discountedItems));
                        volumes.Remove(volumes[1]);
                        volumes.Remove(volumes[0]);
                    }
                }
            }
            else if(campaign.GetType() == typeof(ComboCampaign))
            {
                var comboCampaign = (ComboCampaign)campaign;
                for (int i=0; i< items.Count(); i++)
                {
                    if (comboCampaign.EanCodes.Contains(items[i].Info))
                    {
                        combos.Add(items[i]);
                    }
                }
                combos = combos.OrderBy(item => item.Price).ToList();

                // Make sure we meet the minimum requirement for the campaign
                while (combos.Count() >= comboCampaign.Quantity)
                {
                    decimal discount = combos[0].Price + combos[1].Price - comboCampaign.Price;
                    if (discount > 0)
                    {
                        discount *= -1;
                        // Reference to what products the discount refer to
                        var discountedItems = new List<CartItem>() { combos[0], combos[1] };
                        ReceiptRows.Add(new Discount(comboCampaign, discount, discountedItems));
                        combos.Remove(combos[1]);
                        combos.Remove(combos[0]);
                    }
                }
            }

        });
        
    }
}

public class ReceiptRow
{
    public string Name { get; set; }
    public string Info { get; set; }
    public decimal Price { get; set; }
}

public class CartItem : ReceiptRow
{
    public int Id { get; set; }
    public CartItem(int id, Item item)
    {
        Id = id;
        Name = item.Name;
        Info = item.EanCode;
        Price = item.Price;
    }
}

public class Item
{
    public string Name { get; set; }
    public string EanCode { get; set; }
    public decimal Price { get; set; }
    public Item(string name, string eanCode, decimal price)
    {
        Name = name;
        EanCode = eanCode;
        Price = price;
    }
}

public class Discount : ReceiptRow
{
    List<CartItem> DiscountedCartItems = new List<CartItem>();
    public Discount(Campaign campaign, decimal price, ICollection<CartItem> cartItems)
    {
        Name = campaign.Name;
        Info = campaign.Description;
        Price = price;
        DiscountedCartItems = cartItems.ToList();
    }
    public List<CartItem> GetDiscountedCartItems()
    {
        return DiscountedCartItems;
    }
}