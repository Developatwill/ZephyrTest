using System;

namespace ZephyrTest
{
    /// <summary>
    /// Structure to hold and evaluate data called from Zephyr API
    /// </summary>
    struct ZephyrData
    {
        public int parm1 { get; set; }
        public int parm2 { get; set; }
        public string op { get; set; }
        public double Evaluate()
        {
            switch (op)
            {
                case "+": return parm1 + parm2;
                case "-": return parm1 - parm2;
                case "*": return parm1 * parm2;
                case "/": return (double)parm1 / parm2;
                default:
                    throw new ArgumentException(
                        string.Format(@"""{0}"" is not a valid operator", op));
            }
        }
    }
}
