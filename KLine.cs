using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpCodeSelect.Model.ExModel
{
    public class KLine
    {
        public string CodeQiHao { get; set; }
        public string CodeNumber { get; set; }
        public List<string> Code350Code { get; set; }
        public bool IsZhong { get; set;  }
        public double KValue { get;set; }
        public Bolling Bolling { get;set;  }
        /// <summary>
        /// 是否在布林中轨之上
        /// </summary>
        public bool IsOverMiddle {
            get
            {
                return KValue >= Bolling.MiddleValue;
            }
        }
        /// <summary>
        /// 当前连中次数
        /// </summary>
        public int CurrentGuaCount { get; set;  }
        /// <summary>
        /// 当前连挂次数
        /// </summary>
        public int CurrentZhongCount { get; set;  }
        
    }
}
