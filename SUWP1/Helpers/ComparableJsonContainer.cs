using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SUWP1.Helpers
{
    class ComparableJsonContainer
    {
        public JsonObject obj { get; set; }
        public long key { get; set; }
    }
}
