//
//  TYPEID_UI.h
//  DoExt_UI
//
//  Created by @userName on @time.
//  Copyright (c) 2015年 DoExt. All rights reserved.
//

#import <UIKit/UIKit.h>

#import "doJsonHelper.h"
#import "doIScriptEngine.h"
#import "doInvokeResult.h"

@protocol do_ImageView_IView <NSObject>

@required

- (void)change_radius: (NSString *)_radius;
- (void)change_enabled: (NSString *)_enabled;
- (void)change_source: (NSString *)_source;
- (void)change_scale: (NSString *)_scale;
- (void)change_cacheType: (NSString *)_cache;

@end
