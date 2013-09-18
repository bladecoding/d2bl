using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Dota2BanlistCore.Db
{
    public partial class BanlistDbContext
    {
        public BanlistDbContext(DbConnection conn)
            : base(conn, true)
        {
        }
    }
}
