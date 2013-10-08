using System;
using System.Collections.Generic;
using EventDriven.Shell.Domain;

namespace EventDriven.Shell
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Portfolio original = Portfolio.Open(PortfolioId.GenerateId(), AccountType.Cheque, Money.Amount(100));
                original.CreditAccount(AccountType.Cheque, Money.Amount(50));
                IEnumerable<DomainEvent> changes = ((IAggregate)original).GetUncommittedEvents();

                Portfolio copy = ActivatorHelper.CreateInstanceUsingNonPublicConstructor<Portfolio>(original.Identity);
                ((IAggregate)copy).LoadFromHistory(changes);
                original.CreditAccount(AccountType.Cheque, Money.Amount(5));

                var copyBalance = copy.GetAccountBalance(AccountType.Cheque);
                var originalBalance = original.GetAccountBalance(AccountType.Cheque);

                if (originalBalance == copyBalance)
                {
                    throw new Exception("Balances should not match");
                }

                if (original != copy)
                {
                    throw new Exception("Aggregates should match by identity");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            Console.ReadKey();
        }
    }
}
