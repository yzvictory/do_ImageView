//
//  TYPEID_UI.h
//  DoExt_UI
//
//  Created by @userName on @time.
//  Copyright (c) 2015年 DoExt. All rights reserved.
//

#import <UIKit/UIKit.h>

#import "doJsonNode.h"
#import "doIScriptEngine.h"
#import "doInvokeResult.h"

@protocol Do_ImageView_IView <NSObject>

@required


- (BOOL)InvokeSyncMethod:(NSString *)_methodName :(doJsonNode *)_dictParas :(id<doIScriptEngine>) _scriptEngine :(doInvokeResult *)_invokeResult;

- (BOOL) InvokeAsyncMethod: (NSString *) _methodName : (doJsonNode *) _dicParas :(id<doIScriptEngine>) _scriptEngine : (NSString *) _callbackFuncName;

- (void)change_radius: (NSString *)_radius;
- (void)change_enabled: (NSString *)_enabled;
- (void)change_source: (NSString *)_source;
- (void)change_scale: (NSString *)_scale;
- (void)change_cache: (NSString *)_cache;






@end
