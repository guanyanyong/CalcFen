using CpCodeSelect.Model.ExModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace CpCodeSelect.Model
{
    public class Hou3Select350_ZhouQiZhong: BaseCodeInfo
    {
		/// <summary>
        /// 周期数,在周期内开出算正常开出
		/// 遗漏数大于等于周期值,表示周期内挂
        /// </summary>
		public int ZhouQiNumber{get;set;}=2;
        /// <summary>
        /// 350个号码
        /// </summary>
        public List<string> Number350 { get; set; }
        /// <summary>
        /// 中之前挂的次数
        /// </summary>
        public int ZhongBeforeGua { get; set; } = 0;
        /// <summary>
        /// 3中之前挂的次数
        /// </summary>
        public int Zhong3BeforeGua { get; set; } = 0;
        /// <summary>
        /// 2中之前挂的次数
        /// </summary>
        public int Zhong2BeforeGua { get;set; } = 0;
        /// <summary>
        /// 当前是否在周期内的中后周期
        /// </summary>
        public bool IsZhouQiZhongHou { get; set; } = false;
        /// <summary>
        /// 周期内中后周期内挂的次数
        /// </summary>
        public int ZhouQiZhongHouGua { get; set; } = 0;
        /// <summary>
        /// K线列表
        /// </summary>

        public List<KLine> KLineList { get; set; }
    }
}
