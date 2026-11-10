using System;
using System.Collections.Generic;
using System.Linq;
namespace AccountBalanceVerification.Core
{
    public sealed class Signatory
    {
        public string Role { get; set; } = "";
        public string Name { get; set; } = "";
        public string Placement { get; set; } = "";
        public bool? OnTitle { get; set; }
        public bool? OnAccounts { get; set; }
        public string DecreeRole { get; set; } = "";
        public string RecapRole { get; set; } = "";
    }
    public static class PrintPlan
    {
        public const string FirstPage = "Prvá strana";
        public const string Accounts = "Na konci každého účtu";
        public const string All = "Všetky strany";
        public static readonly string[] Placements = { FirstPage, Accounts, All };
        public static readonly string[] DefaultRoles = { "Predseda", "Člen", "Starosta", "Primátor", "Zodpovedná osoba" };
        public static List<Signatory> Select(IEnumerable<Signatory> people, bool title)
        {
            var populated = people.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();
            if (populated.Any(x => string.IsNullOrWhiteSpace(x.Role) ||
                (!(x.OnTitle.HasValue && x.OnAccounts.HasValue) && !Placements.Contains(x.Placement))))
                throw new InvalidOperationException("Pri každom mene vyplňte funkciu a platnú možnosť tlače na hárku Parametre.");
            return populated.Where(x => (title ? x.OnTitle : x.OnAccounts) ??
                (x.Placement == All || x.Placement == (title ? FirstPage : Accounts))).ToList();
        }
    }
}
