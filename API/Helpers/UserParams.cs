using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class UserParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int _defaultPageSize = 10;

        public int PageSize
        {
            get => _defaultPageSize;
            set => _defaultPageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public string CurrentUsername { get; set; }
        public string Gender { get; set; }
    }
}