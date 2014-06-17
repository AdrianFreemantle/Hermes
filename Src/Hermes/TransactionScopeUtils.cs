﻿using System;
using System.Transactions;

// ReSharper disable CheckNamespace
namespace System.Transactions
// ReSharper restore CheckNamespace
{
    public class TransactionScopeUtils
    {
        public static TimeSpan Timeout { get; set; }

        static TransactionScopeUtils()
        {
            #if DEBUG
            Timeout = TimeSpan.FromMinutes(5);
            #else
            Timeout = TimeSpan.FromMinutes(1);
            #endif
        }

        public static TransactionScope Begin()
        {
            return Begin(TransactionScopeOption.Required);
        }

        public static TransactionScope Begin(TransactionScopeOption scopeOption)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = Timeout
            };

            return new TransactionScope(scopeOption, transactionOptions);
        }

        public static TransactionScope Begin(TransactionScopeOption scopeOption, TimeSpan timeOut)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = timeOut
            };

            return new TransactionScope(scopeOption, transactionOptions);
        }
    } 
}