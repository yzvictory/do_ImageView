package extimplement;

import java.util.Map;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.graphics.Rect;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.ColorDrawable;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.ImageView;
import core.helper.DoIOHelper;
import core.helper.DoImageHandleHelper;
import core.helper.DoImageLoadHelper;
import core.helper.DoImageLoadHelper.OnPostExecuteListener;
import core.helper.DoTextHelper;
import core.helper.DoUIModuleHelper;
import core.helper.jsonparse.DoJsonNode;
import core.interfaces.DoIScriptEngine;
import core.interfaces.DoIUIModuleView;
import core.object.DoInvokeResult;
import core.object.DoUIModule;
import extdefine.do_ImageView_IMethod;
import extdefine.do_ImageView_MAbstract;

/**
 * 自定义扩展UIView组件实现类，此类必须继承相应VIEW类，并实现DoIUIModuleView,Do_ImageView_IMethod接口；
 * #如何调用组件自定义事件？可以通过如下方法触发事件：
 * this.model.getEventCenter().fireEvent(_messageName, jsonResult);
 * 参数解释：@_messageName字符串事件名称，@jsonResult传递事件参数对象； 获取DoInvokeResult对象方式new
 * DoInvokeResult(this.model.getUniqueKey());
 */
public class do_ImageView_View extends ImageView implements DoIUIModuleView, do_ImageView_IMethod, OnClickListener {

	/**
	 * 每个UIview都会引用一个具体的model实例；
	 */
	private do_ImageView_MAbstract model;
	private ColorDrawable bgColorDrawable = new ColorDrawable(Color.TRANSPARENT);
	private float radius;

	public do_ImageView_View(Context context) {
		super(context);
		this.setScaleType(ScaleType.FIT_XY);
		this.setEnabled(false);
	}

	public void setRadius(float radius) {
		this.radius = radius;
	}

	private float getRadius() {
		return (float) (this.radius * Math.min(this.model.getXZoom(), this.model.getYZoom()));
	}

	@Override
	public void setBackgroundColor(int color) {
		bgColorDrawable = new ColorDrawable(color);
		this.postInvalidate();
	}

	@Override
	protected void onDraw(Canvas canvas) {
		Bitmap bgBitmap = DoImageHandleHelper.drawableToBitmap(bgColorDrawable, getWidth(), getHeight());
		Bitmap newBitmap = Bitmap.createBitmap(bgBitmap.getWidth(), bgBitmap.getHeight(), Bitmap.Config.RGB_565);
		Canvas newCanvas = new Canvas(newBitmap);
		newCanvas.drawBitmap(bgBitmap, 0, 0, new Paint());
		if (getDrawable() != null) {
			Bitmap imageBitmap = ((BitmapDrawable) getDrawable()).getBitmap();
			if (imageBitmap != null) {
				Bitmap scaledBitmap = getScaledBitmap(imageBitmap);
				float left = Math.abs(bgBitmap.getWidth() - scaledBitmap.getWidth()) / 2;
				float top = Math.abs(bgBitmap.getHeight() - scaledBitmap.getHeight()) / 2;
				newCanvas.drawBitmap(scaledBitmap, left, top, new Paint());
			}
		}
		newCanvas.save();
		newCanvas.restore();
		canvas.drawBitmap(createRadiusBitmap(newBitmap), 0, 0, new Paint());
	}

	private Bitmap getScaledBitmap(Bitmap imageBitmap) {
		// 获取原图真实宽、高
		Rect rect = getDrawable().getBounds();
		int dw = rect.width();
		int dh = rect.height();
		// 获得ImageView中Image的变换矩阵
		Matrix m = getImageMatrix();
		float[] values = new float[10];
		m.getValues(values);
		// Image在绘制过程中的变换矩阵，从中获得x和y方向的缩放系数
		float sx = values[0];
		float sy = values[4];
		// 计算Image在屏幕上实际绘制的宽高
		int cw = (int) (dw * sx);
		int ch = (int) (dh * sy);
		if(cw > getWidth() || ch > getHeight()){
			return createCenterTypeScaledBitmap(imageBitmap, cw, ch);
		}
		return Bitmap.createScaledBitmap(imageBitmap, cw, ch, true);
	}
	
	private Bitmap createCenterTypeScaledBitmap(Bitmap bitmap, int cw, int ch){
		Bitmap centerScaleBitmap = Bitmap.createBitmap(getWidth(), getHeight(),
				Bitmap.Config.RGB_565);
		Canvas canvas = new Canvas(centerScaleBitmap);
		Rect src = new Rect();
		int bx = (cw - getWidth()) / 2;
		int by = (ch - getHeight()) / 2;
		bx = bx < 0 ? 0 : bx;
		by = by < 0 ? 0 : by;
		src.left = bx;
		src.top = by;
		src.right = bx + getWidth();
		src.bottom = by + getHeight();
		canvas.drawBitmap(bitmap, src, 
				new Rect(0, 0, getWidth(), getHeight()), new Paint());
		return centerScaleBitmap;
	} 

	private Bitmap createRadiusBitmap(Bitmap bitmap) {
		if (this.radius > 0f) {
			return DoImageHandleHelper.createRoundBitmap(bitmap, getRadius());
		}
		return bitmap;
	}

	@Override
	public void onClick(View v) {
		DoInvokeResult _invokeResult = new DoInvokeResult(this.model.getUniqueKey());
		this.model.getEventCenter().fireEvent("touch", _invokeResult);
	}

	/**
	 * 初始化加载view准备,_doUIModule是对应当前UIView的model实例
	 */
	@Override
	public void loadView(DoUIModule _doUIModule) throws Exception {
		this.model = (do_ImageView_MAbstract) _doUIModule;
		this.setOnClickListener(this);
	}

	/**
	 * 动态修改属性值时会被调用，方法返回值为true表示赋值有效，并执行onPropertiesChanged，否则不进行赋值；
	 * 
	 * @_changedValues<key,value>属性集（key名称、value值）；
	 */
	@Override
	public boolean onPropertiesChanging(Map<String, String> _changedValues) {
		return true;
	}

	/**
	 * 属性赋值成功后被调用，可以根据组件定义相关属性值修改UIView可视化操作；
	 * 
	 * @_changedValues<key,value>属性集（key名称、value值）；
	 */
	@Override
	public void onPropertiesChanged(Map<String, String> _changedValues) {
		DoUIModuleHelper.handleBasicViewProperChanged(this.model, _changedValues);
		if (_changedValues.containsKey("radius")) {
			setRadius(DoTextHelper.strToFloat(_changedValues.get("radius"), 0f));
		}
		if (_changedValues.containsKey("enabled")) {
			this.setEnabled(Boolean.parseBoolean(_changedValues.get("enabled")));
		}
		if (_changedValues.containsKey("scale")) {
			String value = _changedValues.get("scale");
			if ("center".equals(value)) {
				this.setScaleType(ScaleType.CENTER);
			} else if ("fillxory".equals(value)) {
				this.setScaleType(ScaleType.FIT_CENTER);
			} else {
				this.setScaleType(ScaleType.FIT_XY);
			}
		}
		if (_changedValues.containsKey("source") || _changedValues.containsKey("cache")) {
			String cache = _changedValues.get("cache");
			String source = _changedValues.get("source");
			try {
				if (null != DoIOHelper.getHttpUrlPath(source)) {
					DoImageLoadHelper.getInstance().loadURL(source, cache, new OnPostExecuteListener() {
						@Override
						public void onPostExecute(Bitmap bitmap) {
							setImageBitmap(bitmap);
						}
					});
				} else {
					String path = DoIOHelper.getLocalFileFullPath(this.model.getCurrentPage().getCurrentApp(), source);
					Bitmap bitmap = DoImageLoadHelper.getInstance().loadLocal(path);
					if(bitmap != null){
						setImageBitmap(bitmap);
					}
				}
			} catch (Exception e) {
				throw new RuntimeException(e);
			}
		}
	}

	/**
	 * 同步方法，JS脚本调用该组件对象方法时会被调用，可以根据_methodName调用相应的接口实现方法；
	 * 
	 * @_methodName 方法名称
	 * @_dictParas 参数（K,V）
	 * @_scriptEngine 当前Page JS上下文环境对象
	 * @_invokeResult 用于返回方法结果对象
	 */
	@Override
	public boolean invokeSyncMethod(String _methodName, DoJsonNode _dictParas, DoIScriptEngine _scriptEngine, DoInvokeResult _invokeResult) throws Exception {
		// ...do something
		return false;
	}

	/**
	 * 异步方法（通常都处理些耗时操作，避免UI线程阻塞），JS脚本调用该组件对象方法时会被调用， 可以根据_methodName调用相应的接口实现方法；
	 * 
	 * @_methodName 方法名称
	 * @_dictParas 参数（K,V）
	 * @_scriptEngine 当前page JS上下文环境
	 * @_callbackFuncName 回调函数名 #如何执行异步方法回调？可以通过如下方法：
	 * _scriptEngine.callback(_callbackFuncName,_invokeResult);
	 * 参数解释：@_callbackFuncName回调函数名，@_invokeResult传递回调函数参数对象；
	 * 获取DoInvokeResult对象方式new DoInvokeResult(this.model.getUniqueKey());
	 */
	@Override
	public boolean invokeAsyncMethod(String _methodName, DoJsonNode _dictParas, DoIScriptEngine _scriptEngine, String _callbackFuncName) {
		// ...do something
		return false;
	}

	/**
	 * 释放资源处理，前端JS脚本调用closePage或执行removeui时会被调用；
	 */
	@Override
	public void onDispose() {
		// ...do something
	}

	/**
	 * 重绘组件，构造组件时由系统框架自动调用；
	 * 或者由前端JS脚本调用组件onRedraw方法时被调用（注：通常是需要动态改变组件（X、Y、Width、Height）属性时手动调用）
	 */
	@Override
	public void onRedraw() {
		this.setLayoutParams(DoUIModuleHelper.getLayoutParams(this.model));
	}

	/**
	 * 获取当前model实例
	 */
	@Override
	public DoUIModule getModel() {
		return model;
	}
}
