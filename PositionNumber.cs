using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpCodeSelect.Model
{
    public class PositionNumber
    {
        /// <summary>
        /// 位置类型
        /// </summary>
        public PositionType PositionType { get; set; }
        /// <summary>
        /// 号码
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 大小类型
        /// </summary>
        public DaXiaoType DaXiao { get; set; }
        /// <summary>
        /// 单双类型
        /// </summary>
        public DanShuangType DanShuang { get; set; }

        #region 大小统计
        /// <summary>
        /// 大连开次数
        /// </summary>
        public int DaLianKai { get; set; }
        /// <summary>
        /// 小连开次数
        /// </summary>
        public int XiaoLianKai { get; set; }
        /// <summary>
        /// 大小推荐号码
        /// </summary>
        public string DaXiaoTuijianNumber { get; set; }

        /// <summary>
        /// 是否大小连开后挂
        /// </summary>
        public bool IsDaXiaoLianKaiGua { get; set; }
        /// <summary>
        /// 大小连开后挂次数
        /// </summary>
        public int DaXiaoLianKaiGuaCount { get; set; }
        /// <summary>
        /// 大小连开挂推荐数值
        /// </summary>
        public string DaXiaoLianGuaTuiJianNumber { get; set; }
        /// <summary>
        /// 大小连开挂推荐的参考数值
        /// 比如:大小小大大小大小小大
        /// </summary>
        public string DaXiaoLianGuaTuiJianCanKao { get; set; }
        #endregion

        #region 单双统计
        /// <summary>
        /// 单连开次数
        /// </summary>
        public int DanLianKai { get; set; }
        /// <summary>
        /// 双连开次数
        /// </summary>
        public int ShuangLianKai { get; set; }
        /// <summary>
        /// 是否单双连开后挂
        /// </summary>
        public bool IsDanShuangLianKaiGua { get; set; }
        /// <summary>
        /// 单双连开后挂次数
        /// </summary>
        public int DanShuangLianKaiGuaCount { get; set; }
        #endregion


    }
}
