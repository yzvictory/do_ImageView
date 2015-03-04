//
//  TYPEID_View.m
//  DoExt_UI
//
//  Created by @userName on @time.
//  Copyright (c) 2015年 DoExt. All rights reserved.
//

#import "Do_ImageView_UIView.h"

#import "doInvokeResult.h"
#import "doIPage.h"
#import "doIScriptEngine.h"
#import "doUIModuleHelper.h"
#import "doScriptEngineHelper.h"
#import "NSData+DoBase64.h"
#import "doUIContainer.h"
#import "doTextHelper.h"
#import "doISourceFS.h"
#import "doServiceContainer.h"
#import "doIOHelper.h"
#import "doIGlobal.h"
#import <CommonCrypto/CommonDigest.h>

@implementation Do_ImageView_UIView


- (instancetype)init
{
    if (self = [super init])
    {
        self.clipsToBounds = YES;
        self.userInteractionEnabled = false;
    }
    return self;
}
#pragma mark - doIUIModuleView协议方法（必须）
//引用Model对象
- (void) LoadView: (doUIModule *) _doUIModule
{
   model = (typeof(model)) _doUIModule;
    
}
//销毁所有的全局对象
- (void) OnDispose
{
    model = nil;
    //自定义的全局属性
}
//实现布局
- (void) OnRedraw
{
    //实现布局相关的修改
    
    //重新调整视图的x,y,w,h
    [doUIModuleHelper OnRedraw:model];
}

#pragma mark - TYPEID_IView协议方法（必须）
#pragma mark - Changed_属性
/*
 如果在Model及父类中注册过 "属性"，可用这种方法获取
 NSString *属性名 = [(doUIModule *)_model GetPropertyValue:@"属性名"];
 
 获取属性最初的默认值
 NSString *属性名 = [(doUIModule *)_model GetProperty:@"属性名"].DefaultValue;
 */
- (void)change_radius: (NSString *)_radius
{
    if (_radius == nil || _radius.length <= 0)
    {
        _radius = [model GetProperty:@"radius"].DefaultValue;
    }
    if ([_radius intValue]< 0)
    {
        NSInteger roundRadius = fabs([[doTextHelper Instance]StrToInt:_radius :0]);
        
        CGPoint center = self.center;  //保存中点
        roundRadius = 2 * roundRadius * model.CurrentUIContainer.InnerXZoom;
        CGRect frame = CGRectMake(0, 0, roundRadius, roundRadius);
        self.bounds = frame;
        self.center = center;
        self.layer.masksToBounds = YES;
        self.layer.cornerRadius = roundRadius / 2;
        self.clipsToBounds = YES;
    }
    else
    {
        self.layer.cornerRadius = [[doTextHelper Instance] StrToInt:_radius :0] * model.CurrentUIContainer.InnerXZoom;
    }
}

- (void)change_enabled: (NSString *)_enabled
{
    if (_enabled == nil || _enabled.length <= 0)
    {
        _enabled = [model GetProperty:@"enabled"].DefaultValue;
    }
    if ( _enabled != nil  && ([_enabled isEqualToString:@"true"] ||[_enabled isEqualToString:@"1"]))
    {
        self.userInteractionEnabled = YES;
    }
    else
    {
        self.userInteractionEnabled = NO;
    }
}

- (void)change_source: (NSString *)_source
{
    if (_source != nil && _source.length > 0)
    {
        
        if ([_source hasPrefix:@"http"])  //如果是由网络请求
        {
            if ([self.cacheType isEqualToString:@"always"])
            {
                self.image = [self getImageFromCache:_source];
                if (self.image == nil)
                {
                    [self getImgFromNetworkAndCached:_source];
                }
            }
            else if ([self.cacheType isEqualToString:@"temporay"])
            {
                self.image = [self getImageFromCache:_source];
                [self getImgFromNetworkAndCached:_source];
            }
            else
            {
                self.image = [self getImageFromNetwork:_source];
            }
        }
        else  //如果是本地文件
        {
            NSString * imgPath = [doIOHelper GetLocalFileFullPath:model.CurrentPage.CurrentApp :_source];
            UIImage * img = [UIImage imageWithContentsOfFile:imgPath];
            
            if (img != nil) {
                self.image = img;
            }
        }
    }
}

- (void)change_scale: (NSString *)_scale
{
    if (_scale == nil || _scale.length <= 0)
    {
        _scale = [model GetProperty:@"scale"].DefaultValue;
    }
    if (_scale != nil && _scale.length > 0)
    {
        if ([_scale.lowercaseString isEqualToString:@"fillxy"])
        {
            self.contentMode = UIViewContentModeScaleToFill;
        }
        else if ([_scale.lowercaseString isEqualToString:@"center"])
        {
            self.contentMode = UIViewContentModeCenter;
        }
        else if ([_scale.lowercaseString isEqualToString:@"fillxory"])
        {
            self.contentMode = UIViewContentModeScaleAspectFit;
        }
    }
}

- (void)change_cache: (NSString *)_cache
{
    if (_cache == nil || _cache.length <= 0)
    {
        _cache = [model GetProperty:@"cache"].DefaultValue;
    }
    if (_cache != nil && _cache.length > 0)
    {
        self.cacheType = _cache;
    }
}

#pragma mark -
#pragma mark - override UIView method
- (void)touchesBegan:(NSSet *)touches withEvent:(UIEvent *)event
{
    [super touchesBegan:touches withEvent:event];
    doInvokeResult* _invokeResult = [[doInvokeResult alloc]init:model.UniqueKey];
    [model.EventCenter FireEvent:@"touch":_invokeResult];
}

#pragma mark -
#pragma mark - private
- (UIImage *)getImageFromNetwork :(NSString *)path
{
    __block UIImage *img;
    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
        NSString *str = [path stringByAddingPercentEscapesUsingEncoding: NSUTF8StringEncoding];
        NSData *dataImg = [NSData dataWithContentsOfURL:[NSURL URLWithString:str]];
        img = [UIImage imageWithData:dataImg];
        if (img) {
            dispatch_async(dispatch_get_main_queue(), ^{
                self.image = img;
            });
        }
    });
    return img;
}

- (void)getImgFromNetworkAndCached :(NSString *)path
{
    self.image = [self getImageFromNetwork:path];
    NSString *_dataRoot = [NSString stringWithFormat:@"%@/main/%@/data", [doServiceContainer Instance].Global.DataRootPath, @"app"];
    
    //不存在缓存文件夹，则创建缓存文件夹
    NSString *cachePath = [NSString stringWithFormat:@"%@/sys/imagecache",_dataRoot];
    
    if (![[NSFileManager defaultManager] fileExistsAtPath:cachePath ])
    {
        [[NSFileManager defaultManager] createDirectoryAtPath:cachePath withIntermediateDirectories:YES attributes:nil error:nil];
    }
    
    NSString *strName = [[doTextHelper Instance] MD5:path];
    NSString *filePath = [NSString stringWithFormat:@"%@/%@.png",cachePath,strName];
    
    NSData *dataImg;
    if (UIImagePNGRepresentation(self.image) == nil) {
        
        dataImg = UIImageJPEGRepresentation(self.image, 1);
        
    } else {
        
        dataImg = UIImagePNGRepresentation(self.image);
    }
    [[NSFileManager defaultManager] createFileAtPath:filePath contents:dataImg attributes:nil];
}

- (UIImage *)getImageFromCache :(NSString *)path
{
    NSString *_dataRoot = [NSString stringWithFormat:@"%@/main/%@/data", [doServiceContainer Instance].Global.DataRootPath ,@"app"];
    
    //不存在缓存文件夹，则创建缓存文件夹
    NSString *cachePath = [NSString stringWithFormat:@"%@/sys/imagecache",_dataRoot];
    NSString *strName = [[doTextHelper Instance] MD5:path];
    NSString *filePath = [NSString stringWithFormat:@"%@/%@.png",cachePath,strName];
    //在本地cache中找到图片
    if ([[NSFileManager defaultManager] fileExistsAtPath:filePath])
    {
        return [UIImage imageWithContentsOfFile:filePath];
    }
    else
    {
        return nil;
    }
}

#pragma mark - doIUIModuleView协议方法（必须）<大部分情况不需修改>
- (BOOL)InvokeSyncMethod:(NSString *)_methodName :(doJsonNode *)_dicParas :(id<doIScriptEngine>)_scriptEngine :(doInvokeResult *)_invokeResult
{
    return [doScriptEngineHelper InvokeSyncSelector:self :_methodName :_dicParas :_scriptEngine :_invokeResult];
}

- (BOOL)InvokeAsyncMethod:(NSString *)_methodName :(doJsonNode *)_dicParas :(id<doIScriptEngine>)_scriptEngine :(NSString *)_callbackFuncName
{
    return [doScriptEngineHelper InvokeASyncSelector:self :_methodName :_dicParas :_scriptEngine :_callbackFuncName];
}

- (BOOL) OnPropertiesChanging: (NSMutableDictionary *) _changedValues
{
    //属性改变时,返回NO，将不会执行Changed方法
    return YES;
}
- (void) OnPropertiesChanged: (NSMutableDictionary*) _changedValues
{
    //_model的属性进行修改，同时调用self的对应的属性方法，修改视图
    [doUIModuleHelper HandleViewProperChanged: self :model : _changedValues ];
}
- (doUIModule *) GetModel
{
    //获取model对象
    return model;
}

@end
