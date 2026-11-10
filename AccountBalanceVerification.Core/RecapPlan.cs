using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountBalanceVerification.Core
{
    public sealed class RecapItem
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string[] Patterns { get; set; }
        public int Sign { get; set; } = 1;
        // Net, Debit or Credit: split mixed accounts per analytical closing balance.
        public string BalanceSide { get; set; } = "Net";
        public string Note { get; set; } = "";
    }
    public sealed class RecapSection
    {
        public string Title { get; set; }
        public bool RollForward { get; set; }
        public List<RecapItem> Items { get; set; } = new List<RecapItem>();
    }
    public static class RecapPlan
    {
        public static bool IsKamienka(VerificationPackage p) => p.Ico == "00323110" && p.FiscalYear == 2025;
        public static List<RecapSection> Build(VerificationPackage p)
        {
            bool sample = IsKamienka(p);
            var result = new List<RecapSection>();
            RecapSection section = null;
            Action<string, bool> addSection = (title, roll) => { section = new RecapSection { Title = title, RollForward = roll }; result.Add(section); };
            Action<string, string, string, int> add = (key, label, patterns, sign) => section.Items.Add(new RecapItem {
                Key = key, Label = label, Patterns = patterns.Split('|'), Sign = sign });
            Action<string, string, string> custom = (key, label, patterns) => {
                add(key, label, sample ? patterns : "", 1);
                section.Items.Last().Note = sample ? "Predbežné mapovanie Kamienka 2025 – potvrdiť s audítorom." : "Vyplňte analytické účty alebo ručné hodnoty.";
            };
            addSection("1a. Dlhodobý majetok", true);
            add("01", "Dlhodobý nehmotný majetok – ÚS 01", "01*", 1);
            add("0203", "Dlhodobý hmotný majetok – ÚS 02 a 03", "02*|03*", 1);
            add("06", "Dlhodobý finančný majetok – ÚS 06", "06*", 1);
            addSection("1b. Oprávky k dlhodobému majetku", true);
            add("07", "Oprávky k DNM – ÚS 07", "07*", -1); add("08", "Oprávky k DHM – ÚS 08", "08*", -1);
            addSection("1c. Opravné položky k dlhodobému majetku", true);
            foreach (var code in new[] { "091", "092", "096" }) add(code, "Opravné položky – " + code, code + "*", -1);
            addSection("2a. Obstaranie dlhodobého majetku", true);
            foreach (var code in new[] { "041", "042", "043" }) add(code, "Obstaranie majetku – " + code, code + "*", 1);
            addSection("2b. Opravné položky k obstaraniu", true);
            foreach (var code in new[] { "093", "094" }) add(code, "Opravné položky – " + code, code + "*", -1);
            addSection("3a. Zásoby", true);
            custom("112M", "Materiál (vrátane nádob TKO)", "1120510"); custom("112P", "Potraviny", "11201");
            custom("112O", "Ostatné zásoby", "119*|12*|13*");
            addSection("3b. Opravné položky k zásobám", true); add("19", "Opravné položky – ÚS 19", "19*", -1);
            addSection("3c. Zúčtovanie so subjektmi verejnej správy – aktíva", false);
            foreach (var code in new[] { "351", "354", "357", "359" }) { add(code + "D", code, code + "*", 1); section.Items.Last().BalanceSide = "Debit"; }
            addSection("3d. Pohľadávky", false);
            foreach (var code in new[] { "314", "315", "318", "319", "335", "378" }) { add(code + "D", code, code + "*", 1); section.Items.Last().BalanceSide = "Debit"; }
            addSection("3e. Opravné položky k pohľadávkam", false);
            custom("391318", "391 – k nedaňovým pohľadávkam", "391318"); section.Items.Last().Sign = -1;
            custom("391319", "391 – k daňovým pohľadávkam", "391319"); section.Items.Last().Sign = -1;
            addSection("3f. Finančný majetok", false);
            custom("211B", "Pokladnica – bytové domy", "21120"); custom("211O", "Pokladnica – obecný úrad", "21110");
            add("213", "213 – Ceniny", "213*", 1);
            custom("221V", "Bankový účet – VÚB bežný", "22110"); custom("221P", "Bankový účet – Prima bežný", "22111");
            custom("221F", "Bankový účet – potravinový", "22113"); custom("221S", "Bankový účet – sociálny fond", "22114");
            custom("221G", "Bankový účet – depozitný / grantový", "22150"); custom("221L", "Bankový účet – SLSP", "22116");
            custom("221B", "Bankové účty – bytové domy (netto)", "221800|221801"); add("261", "261 – Peniaze na ceste", "261*", 1);
            addSection("3g. Poskytnuté návratné finančné výpomoci", false);
            foreach (var code in new[] { "271", "272", "274", "275", "277" }) add(code, code, code + "*", 1);
            addSection("4. Vlastné imanie", false);
            foreach (var code in new[] { "414", "415", "428" }) add(code, code, code + "*", -1);
            add("RESULT", "Výsledok hospodárenia (výnosy − náklady)", "5*|6*", -1);
            section.Items.Last().Note = "Pred uzávierkou: netto triedy 6 mínus trieda 5. Overiť úplnosť zdroja; pri uzavretých účtoch zadať overený výsledok ručne.";
            if (!p.Rows.Any(x => x.AccountNo.StartsWith("5")) || !p.Rows.Any(x => x.AccountNo.StartsWith("6"))) section.Items.Last().Patterns = new[] { "" };
            addSection("5a. Rezervy", true);
            add("323", "Krátkodobé rezervy – 323", "323*", -1); add("451459", "Dlhodobé rezervy – 451, 459", "451*|459*", -1);
            addSection("5b. Zúčtovanie so subjektmi verejnej správy – pasíva", false);
            foreach (var code in new[] { "357", "359" }) { add(code + "C", code, code + "*", -1); section.Items.Last().BalanceSide = "Credit"; }
            addSection("5c. Dlhodobé záväzky", false);
            foreach (var code in new[] { "472", "474", "475", "479" }) add(code, code, code + "*", -1);
            addSection("5d. Krátkodobé záväzky", false);
            foreach (var code in new[] { "321", "324", "325", "326", "331", "333", "335", "336", "342", "372", "379" }) {
                add(code + "C", code, code + "*", -1); section.Items.Last().BalanceSide = "Credit";
            }
            addSection("5e. Bankové úvery a prijaté výpomoci", false);
            add("231", "231 – Krátkodobé bankové úvery", "231*", -1); add("461", "461 – Bankové úvery spolu", "461*", -1);
            add("273", "273 – Prijaté návratné finančné výpomoci", "273*", -1);
            addSection("5f. Časové rozlíšenie nákladov a výnosov", false);
            foreach (var code in new[] { "381", "382", "383", "384", "385" }) add(code, code, code + "*", code == "383" || code == "384" ? -1 : 1);
            addSection("6. Podsúvahové účty", true);
            custom("751", "751 – podľa členenia správy", "7511|7512");
            custom("752", "752 – podľa členenia správy", "752020|752021|752022|752023|752024|752025");
            custom("753", "753 – podľa členenia správy", "753*");
            return result;
        }
        public static string NormalizeAccount(string code) => new string((code ?? "").Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
}
