using GmailInvoiceAnalyzer.Models;
using GmailInvoiceAnalyzer.Services;
using System.Text.RegularExpressions;

namespace GmailInvoiceAnalyzer.PDF.Helpers
{
    public class InvoiceHelper
    {
        private readonly AiAnalyzer _aiAnalyzer;
        public InvoiceHelper(AiAnalyzer aiAnalyzer)
        {
            _aiAnalyzer = aiAnalyzer;
        }

        public async Task<string> CategorizeItem(string description)
        {
            var d = description.ToLower();

            // -------- CLEANING SUPPLIES / ASEO DE HOGAR --------
            if (d.Contains("mistolin") || d.Contains("clorox") || d.Contains("detergente") ||
                d.Contains("limpiador") || d.Contains("lavaplatos") || d.Contains("desinfectante") ||
                d.Contains("suavizante") || d.Contains("multiuso") || d.Contains("bleach") ||
                d.Contains("limpia") || d.Contains("sanitizante"))
                return "Cleaning Supplies";

            // -------- PERSONAL CARE / HIGIENE PERSONAL --------
            if (d.Contains("jabon") || d.Contains("jabón") || d.Contains("shampoo") ||
                d.Contains("shampu") || d.Contains("desodorante") || d.Contains("afeitadora") ||
                d.Contains("pañal") || d.Contains("toalla femenina") || d.Contains("crema dental") ||
                d.Contains("cepillo dental") || d.Contains("rinã") || d.Contains("rinna") ||
                d.Contains("riná") || d.Contains("rina"))
                return "Hygiene";

            // -------- DAIRY / LÁCTEOS --------
            if (d.Contains("leche") || d.Contains("queso") || d.Contains("yogurt") ||
                d.Contains("mantequilla") || d.Contains("margarina"))
                return "Dairy";

            // -------- PROTEINS / MEATS --------
            if (d.Contains("pollo") || d.Contains("res") || d.Contains("carne") || d.Contains("hígado") ||
                d.Contains("chuleta") || d.Contains("pescado") || d.Contains("atun") ||
                d.Contains("atún") || d.Contains("cerdo") || d.Contains("molida") ||
                d.Contains("picado"))
                return "Proteins";

            // -------- DRY FOOD / ABARROTES SECOS --------
            if (d.Contains("arroz") || d.Contains("pasta") || d.Contains("espaguetti") ||
                d.Contains("espagueti") || d.Contains("fideo") || d.Contains("harina") ||
                d.Contains("avena") || d.Contains("granos") || d.Contains("trigo"))
                return "Dry Food";

            // -------- CANNED GOODS / ENLATADOS --------
            if (d.Contains("sardina") || d.Contains("jamonilla") || d.Contains("guisantes") ||
                d.Contains("veget") || d.Contains("mix") || d.Contains("pasta tomate") ||
                d.Contains("salsa tomate") || d.Contains("margarin") || d.Contains("tuna") ||
                d.Contains("atún") || d.Contains("maiz"))
                return "Canned Goods";

            // -------- BEVERAGES / BEBIDAS --------
            if (d.Contains("café") || d.Contains("cafe") || d.Contains("té") || d.Contains("te") ||
                d.Contains("jugo") || d.Contains("bebida") || d.Contains("refresco"))
                return "Drinks";

            // -------- BAKERY / PANADERÍA --------
            if (d.Contains("pan") || d.Contains("pullman") || d.Contains("pita"))
                return "Bakery";

            // -------- BABY PRODUCTS --------
            if (d.Contains("bebe") || d.Contains("bebé") || d.Contains("baby"))
                return "Baby";

            // -------- CEREALS --------
            if (d.Contains("cereal") || d.Contains("corn flakes"))
                return "Cereals";

            // -------- SAUCES / Salsas --------
            if (d.Contains("salsa") || d.Contains("vinagre") || d.Contains("oregano") ||
                d.Contains("orégano") || d.Contains("ketchup") || d.Contains("mostaza") ||
                d.Contains("condimento"))
                return "Sauces";

            // -------- PET SUPPLIES --------
            if (d.Contains("gati") || d.Contains("ascan") || d.Contains("dog") ||
                d.Contains("cat") || d.Contains("mascota"))
                return "Pet Supplies";

            // -------- SNACKS --------
            if (d.Contains("galleta") || d.Contains("chips") || d.Contains("snack") ||
                d.Contains("chocolate") || d.Contains("cracker"))
                return "Snacks";

            // -------- FROZEN --------
            if (d.Contains("congelado") || d.Contains("frozen"))
                return "Frozen";

            var categoryIA = await _aiAnalyzer.CategorizeWithAIAsync(d);
            if (!string.IsNullOrEmpty(categoryIA))
            {
                return categoryIA;
            }
            else
            {
                return "Other";
            }
        }

        public static List<InvoiceItem> ExtractItems(string raw)
        {
            var clean = raw.Replace("\n", " ").Replace("\r", " ");
            clean = Regex.Replace(clean, @"\s+", " ");

            var pattern = @"
        (?<num>\d{3})
        (?<desc>[A-Z0-9 \-\/]+?)
        (?<qty>\d+\.\d{2})
        \s*und
        (?<unit>\d+\.\d{2})
        (?<disc>\d+\.\d{2})
        (?<itbms>\d+\.\d{2})
        (?<line>\d+\.\d{2})
        (?<item>\d+\.\d{2})
    ";

            var matches = Regex.Matches(clean, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            var items = new List<InvoiceItem>();

            foreach (Match m in matches)
            {
                items.Add(new InvoiceItem
                {
                    Number = m.Groups["num"].Value,
                    Description = m.Groups["desc"].Value.Trim(),
                    Quantity = decimal.Parse(m.Groups["qty"].Value),
                    UnitPrice = decimal.Parse(m.Groups["unit"].Value),
                    Discount = decimal.Parse(m.Groups["disc"].Value),
                    ITBMS = decimal.Parse(m.Groups["itbms"].Value),
                    LinePrice = decimal.Parse(m.Groups["line"].Value),
                    ItemPrice = decimal.Parse(m.Groups["item"].Value),
                });
            }

            return items;
        }
    }
}
