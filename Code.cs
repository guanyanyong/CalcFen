using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpCodeSelect.Model
{
    public class Code
    {
        public string CodeQiHao { get; set; }
        public string CodeNumber { get; set; }
        /// <summary>
        /// 上一期号码
        /// </summary>
        public Code PreCode { get; set; }
        public PositionNumber Wan { get; set; }
        public PositionNumber Qian { get; set; }
        public PositionNumber Bai { get; set; }
        public PositionNumber Shi { get; set; }
        public PositionNumber Ge { get; set; }
        /// <summary>
        /// 获取后2字符串
        /// </summary>
        /// <returns></returns>
        public string GetHou2String()
        {
            return $"{Shi.Number}{Ge.Number}";
        }
        /// <summary>
        /// 获取后3字符串
        /// </summary>
        /// <returns></returns>
        public string GetHou3String()
        {
            return $"{Bai.Number}{Shi.Number}{Ge.Number}";
        }
    }
}
