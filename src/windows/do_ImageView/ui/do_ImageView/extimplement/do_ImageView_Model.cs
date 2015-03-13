using doCore.Object;
using do_ImageView.extdefine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace do_ImageView.extimplement
{
    /// <summary>
    /// 自定义扩展组件Model实现，继承@TYPEID_MAbstract抽象类；
    /// </summary>
    public class do_ImageView_Model : do_ImageView_MAbstract
    {
        public do_ImageView_Model()
            : base()
        {
        }
        public override void OnInit()
        {
            base.OnInit();
            this.RegistProperty(new doProperty("enabled", PropertyDataType.Bool, "false", false));
            this.RegistProperty(new doProperty("source", PropertyDataType.String, "", false));
            this.RegistProperty(new doProperty("scale", PropertyDataType.String, "fillxy", false));
            this.RegistProperty(new doProperty("radius", PropertyDataType.Number, "0", false));
            this.RegistProperty(new doProperty("cache", PropertyDataType.String, "always", false));
       
        }
        public override async Task<bool> InvokeAsyncMethod(string _methodName, doCore.Helper.JsonParse.doJsonNode _dictParas, doCore.Interface.doIScriptEngine _scriptEngine, string _callbackFuncName)
        {
            if (await base.InvokeAsyncMethod(_methodName, _dictParas, _scriptEngine, _callbackFuncName)) return true;
            return false;
        }
        public override bool InvokeSyncMethod(string _methodName, doCore.Helper.JsonParse.doJsonNode _dictParas, doCore.Interface.doIScriptEngine _scriptEngine, doCore.Object.doInvokeResult _invokeResult)
        {
            if (base.InvokeSyncMethod(_methodName, _dictParas, _scriptEngine, _invokeResult)) return true;
            return false;
        }
    }
}
