using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpCodeSelect.Model
{
    public enum PositionType
    {
        万=0,
        千=1,
        百=2,
        十=3,
        个=4
    }

    public enum DaXiaoType
    {
        大=0,
        小=1
    }

    public enum DanShuangType
    {
        单=0,
        双=1
    }
    public enum DragonTigerType
    {
        和=0,
        龙=1,
        虎=2

    }
    /// <summary>
    /// 杀3码位置
    /// </summary>

    public enum Kill3Position
    {
        前三=0,
        中三=1,
        后三 =2
    }
    /// <summary>
    /// 组六杀一码位置
    /// </summary>
    public enum Zu6Kill1Position
    {
        前三 = 0,
        中三 = 1,
        后三 = 2
    }
    
}
