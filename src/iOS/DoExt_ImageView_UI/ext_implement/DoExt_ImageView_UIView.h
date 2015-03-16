//
//  TYPEID_View.h
//  DoExt_UI
//
//  Created by @userName on @time.
//  Copyright (c) 2015年 DoExt. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "DoExt_ImageView_IView.h"
#import "DoExt_ImageView_UIModel.h"
#import "doIUIModuleView.h"

@interface DoExt_ImageView_UIView : UIImageView<DoExt_ImageView_IView,doIUIModuleView>
//可根据具体实现替换UIView
{
    @private
    __weak DoExt_ImageView_UIModel *model;
}
@property (nonatomic, strong)NSString *cacheType;

@end
