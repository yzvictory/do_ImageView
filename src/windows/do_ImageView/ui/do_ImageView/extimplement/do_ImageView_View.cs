using doCore.Helper;
using doCore.Helper.JsonParse;
using doCore.Interface;
using doCore.Object;
using do_ImageView.extdefine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace do_ImageView.extimplement
{
    /// <summary>
    /// 自定义扩展UIView组件实现类，此类必须继承相应控件类或UserControl类，并实现doIUIModuleView,@TYPEID_IMethod接口；
    /// #如何调用组件自定义事件？可以通过如下方法触发事件：
    /// this.model.EventCenter.fireEvent(_messageName, jsonResult);
    /// 参数解释：@_messageName字符串事件名称，@jsonResult传递事件参数对象；
    /// 获取doInvokeResult对象方式new doInvokeResult(model.UniqueKey);
    /// </summary>
    public class do_ImageView_View : UserControl, doIUIModuleView, do_ImageView_IMethod
    {
        /// <summary>
        /// 每个UIview都会引用一个具体的model实例；
        /// </summary>
        private do_ImageView_MAbstract model;
        Border bor = new Border();
        public do_ImageView_View()
        {
            this.Content = bor;
        }
        /// <summary>
        /// 初始化加载view准备,_doUIModule是对应当前UIView的model实例
        /// </summary>
        /// <param name="_doComponentUI"></param>
        public void LoadView(doUIModule _doUIModule)
        {
            this.model = (do_ImageView_MAbstract)_doUIModule;
            this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            this.PointerPressed += do_ImageView_View_PointerPressed;
            this.PointerReleased += do_ImageView_View_PointerReleased;
            this.Width =Convert.ToDouble( this.model.GetPropertyValue("width"));
            this.Height = Convert.ToDouble(this.model.GetPropertyValue("height"));
        }

        void do_ImageView_View_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            doInvokeResult _invokeResult = new doInvokeResult(this.model.UniqueKey);
            this.model.EventCenter.FireEvent("touchup", _invokeResult);
            this.model.EventCenter.FireEvent("touch", _invokeResult);
        }

        void do_ImageView_View_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            doInvokeResult _invokeResult = new doInvokeResult(this.model.UniqueKey);
            this.model.EventCenter.FireEvent("touchdown", _invokeResult);
        }

       
        public doUIModule GetModel()
        {
            return this.model;
        }
    
        /// <summary>
        /// 动态修改属性值时会被调用，方法返回值为true表示赋值有效，并执行OnPropertiesChanged，否则不进行赋值；
        /// </summary>
        /// <param name="_changedValues">属性集（key名称、value值）</param>
        /// <returns></returns>
        public bool OnPropertiesChanging(Dictionary<string, string> _changedValues)
        {
            return true;
        }
        /// <summary>
        /// 属性赋值成功后被调用，可以根据组件定义相关属性值修改UIView可视化操作；
        /// </summary>
        /// <param name="_changedValues">属性集（key名称、value值）</param>
        async public void OnPropertiesChanged(Dictionary<string, string> _changedValues)
        {
            doUIModuleHelper.HandleBasicViewProperChanged(this.model, _changedValues);
            if (_changedValues.Keys.Contains("radius"))
            {
                double radius = Convert.ToDouble(_changedValues["radius"]);
                (this.Content as Border).CornerRadius = new CornerRadius(radius);
            }
            if (_changedValues.Keys.Contains("enabled"))
            {
                if (_changedValues["enabled"] == "true")
                {
                    this.IsEnabled = true;
                }
                else
                {
                    this.IsEnabled = false;
                }
            }
          
            if (_changedValues.Keys.Contains("scale"))
            {
                SetImageType(_changedValues["scale"]);
            }
            if (_changedValues.Keys.Contains("source"))
            {
                string cacheTypevalue = "";
                if (!_changedValues.Keys.Contains("cacheType"))
                {
                    cacheTypevalue = model.GetProperty("cacheType").DefaultValue;
                }
                else
                {
                    cacheTypevalue = _changedValues["cacheType"];
                }

                string source = _changedValues["source"];
                if (doIOHelper.GetHttpUrlPath(source) != null)
                {
                    this.SetHttpImage(source, cacheTypevalue);
                }
                else
                {
                    string bgimage = doIOHelper.GetLocalFileFullPath(this.model.CurrentPage.CurrentApp, source);
                    try
                    {
                        ImageBrush ib = await doIOHelper.GetImageBrushFromUrl(bgimage);
                        if (ib != null)
                        {
                            (this.Content as Border).Background = ib;
                        }
                    }
                    catch
                    {

                    }
                }
            }
            if (_changedValues.Keys.Contains("bgColor"))
            {
                (this.Content as Border).Background = doUIModuleHelper.GetColorFromString(_changedValues["bgColor"], new SolidColorBrush());
            }
        }
        private async void SetHttpImage(string path, string cacheType)
        {
            string rootpath = "";
            if (cacheType == "always")
            {
                //表示每次打开这个imageview都会先读缓存的本地图片，然后再读服务器的网络图片，然后再缓存到本地
                rootpath = await doFileCacheHelper.getCachedFileAndRefresh(this.model.CurrentPage.CurrentApp, path);
                setimage(rootpath);
            }
            if (cacheType == "never")
            {
                //表示永远不读本地缓存，永远都是读远程图片
                GetHttpImage(path);
            }
            if (cacheType == "temporary")
            {
                //表示只读本地缓存，缓存没有的时候从远程读取一次然后就缓存到本地
                rootpath = await doFileCacheHelper.getTempCachedFile(this.model.CurrentPage.CurrentApp, path);
                setimage(rootpath);
            }
        }
       private async void setimage(string rootpath)
        {
            if (!string.IsNullOrEmpty(rootpath))
            {
                (this.Content as Border).Background = await doIOHelper.GetImageBrushFromUrl(rootpath);
            }
        }

        private ImageBrush GetHttpImage(string source)
        {
            BitmapImage bitImag = new BitmapImage();
            bitImag.UriSource = new Uri(source, UriKind.Absolute);
            ImageBrush image = new ImageBrush();
            image.ImageSource = bitImag;
            return image;
        }
        private void SetImageType(string type)
        {
            ImageBrush image = new ImageBrush();
            if ((this.Content as Border).Background != null)
            {
                image = (this.Content as Border).Background as ImageBrush;
                switch (type)
                {
                    case "center":
                        image.Stretch = Stretch.Uniform;
                        (this.Content as Border).Background = image;
                        break;
                    case "fillxory":
                        image.Stretch = Stretch.Fill;
                        (this.Content as Border).Background = image;
                        break;
                    default:
                        image.Stretch = Stretch.None;
                        (this.Content as Border).Background = image;
                        break;
                }
            }
        }
  
        /// <summary>
        /// 同步方法，JS脚本调用该组件对象方法时会被调用，可以根据_methodName调用相应的接口实现方法；
        /// </summary>
        /// <param name="_methodName">方法名称</param>
        /// <param name="_dictParas">参数（K,V）</param>
        /// <param name="_scriptEngine">当前Page JS上下文环境对象</param>
        /// <param name="_invokeResult">用于返回方法结果对象</param>
        /// <returns></returns>
        public bool InvokeSyncMethod(string _methodName, doJsonNode _dictParas, doIScriptEngine _scriptEngine, doInvokeResult _invokeResult)
        {

            return false;
        }
        /// <summary>
        /// 异步方法（通常都处理些耗时操作，避免UI线程阻塞），JS脚本调用该组件对象方法时会被调用，
        /// 可以根据_methodName调用相应的接口实现方法；#如何执行异步方法回调？可以通过如下方法：
        /// _scriptEngine.callback(_callbackFuncName, _invokeResult);
        /// 参数解释：@_callbackFuncName回调函数名，@_invokeResult传递回调函数参数对象；
        /// 获取doInvokeResult对象方式new doInvokeResult(model.UniqueKey);
        /// </summary>
        /// <param name="_methodName">方法名称</param>
        /// <param name="_dictParas">参数（K,V）</param>
        /// <param name="_scriptEngine">当前page JS上下文环境</param>
        /// <param name="_callbackFuncName">回调函数名</param>
        /// <returns></returns>
        public bool InvokeAsyncMethod(string _methodName, doJsonNode _dictParas, doIScriptEngine _scriptEngine, string _callbackFuncName)
        {
            return false;
        }
        /// <summary>
        /// 重绘组件，构造组件时由系统框架自动调用；
        /// 或者由前端JS脚本调用组件onRedraw方法时被调用（注：通常是需要动态改变组件（X、Y、Width、Height）属性时手动调用）
        /// </summary>
        public void OnRedraw()
        {
            var tp = doUIModuleHelper.GetThickness(this.model);
            this.Margin = tp.Item1;
            this.Width = tp.Item2;
            this.Height = tp.Item3;
        }
        /// <summary>
        /// 释放资源处理，前端JS脚本调用closePage或执行removeui时会被调用；
        /// </summary>
        public void OnDispose()
        {

        }
    }
}
