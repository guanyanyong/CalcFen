using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpCodeSelect.Model
{
    public class BaseCodeInfo
    {
        /// <summary>
        /// 是否需要中 初始需要
        /// </summary>
        public bool NeedZhong { get; set; } = true;

        /// <summary>
        /// 对应的期号
        /// </summary>
        public string CodeQiHao { get; set; }
        /// <summary>
        /// 对应的期号号码
        /// </summary>
        public string CodeNumber { get; set; }
        /// <summary>
        /// 当前挂的个数
        /// </summary>
        public int GuaCount { get; set; } = 0;
        public int ZhongGount { get; set; } = 0;
    }
}
