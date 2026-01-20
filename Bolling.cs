using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpCodeSelect.Model.ExModel
{
    public class Bolling
    {
        /// <summary>
        /// 布林中轨
        /// </summary>
        public double MiddleValue { get; set; } = 0;
        /// <summary>
        /// 布林上轨
        /// </summary>
        public double BollUpperValue { get; set; } = 0;
        /// <summary>
        /// 布林下轨
        /// </summary>
        public double BollLowerValue { get; set; } = 0;
    }
}
