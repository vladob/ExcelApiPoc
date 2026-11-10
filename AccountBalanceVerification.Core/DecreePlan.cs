using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountBalanceVerification.Core
{
    public static class DecreePlan
    {
        public const string Recipient = "Príjemca", Signer = "Podpisujúci", Commission = "Komisia";

        public static string Appointment(string role)
        {
            switch ((role ?? "").Trim().ToLowerInvariant()) {
                case "predseda": case "predseda komisie": return "predsedu";
                case "predsedníčka": case "predsedníčka komisie": return "predsedníčku";
                case "člen": case "člen komisie": return "člena";
                case "členka": case "členka komisie": return "členku";
                default: return "";
            }
        }

        public static Signatory Migrate(string role, string name, string placement)
        {
            if (!PrintPlan.Placements.Contains(placement) && !string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Neplatná možnosť tlače pri osobe: " + name);
            bool member = Appointment(role).Length > 0;
            return new Signatory { Role = role, Name = name,
                OnTitle = placement == PrintPlan.All || placement == PrintPlan.FirstPage,
                OnAccounts = placement == PrintPlan.All || placement == PrintPlan.Accounts,
                DecreeRole = member ? Recipient : "", RecapRole = member ? Commission : "" };
        }

        public static List<Signatory> Recipients(IEnumerable<Signatory> people) => people
            .Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.DecreeRole == Recipient).ToList();

        public static Signatory Validate(IEnumerable<Signatory> people)
        {
            var rows = people.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();
            if (rows.Any(x => x.DecreeRole != "" && x.DecreeRole != Recipient && x.DecreeRole != Signer))
                throw new InvalidOperationException("Vyberte platnú úlohu v stĺpci Menovací dekrét.");
            var recipients = Recipients(rows);
            if (recipients.Count == 0) throw new InvalidOperationException("Vyberte aspoň jedného príjemcu menovacieho dekrétu.");
            var unknown = recipients.FirstOrDefault(x => Appointment(x.Role).Length == 0);
            if (unknown != null) throw new InvalidOperationException("Pri príjemcovi " + unknown.Name +
                " vyberte funkciu Predseda, Predsedníčka, Člen alebo Členka.");
            var signers = rows.Where(x => x.DecreeRole == Signer).ToList();
            if (signers.Count != 1 || string.IsNullOrWhiteSpace(signers[0].Role))
                throw new InvalidOperationException("Vyberte presne jednu osobu s funkciou, ktorá podpisuje menovacie dekréty.");
            return signers[0];
        }
    }
}
