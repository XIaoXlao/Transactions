using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Models
{
    public class Transactions
    {
        public string TransNo { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
    }
}

//CREATE TABLE[dbo].[Transactions](

//   [TransNo][varchar](50) NOT NULL,

//  [Amount] [decimal](18, 0) NOT NULL,

//  [CurrencyCode] [char](3) NOT NULL,

//  [TransDate] [datetime] NOT NULL,

//  [Status] [varchar](20) NOT NULL
//) ON[PRIMARY]
//GO
