//
//  TYPEID_Model.m
//  DoExt_UI
//
//  Created by @userName on @time.
//  Copyright (c) 2015年 DoExt. All rights reserved.
//

#import "do_ImageView_UIModel.h"
#import "doProperty.h"

@implementation do_ImageView_UIModel

#pragma mark - 注册属性（--属性定义--）
/*
 [self RegistProperty:[[doProperty alloc]init:@"属性名" :属性类型 :@"默认值" : BOOL:是否支持代码修改属性]];
 */
-(void)OnInit
{
    [super OnInit];
    //注册属性
    //radius -- 圆角半径
    //enabled -- 是否可点击
    //source -- 图片路径
    //scale -- 图片显示类型
    //cache -- 是否支持网络图片的本地cache
    [self RegistProperty:[[doProperty alloc]init:@"radius" :Number :@"0" :NO]];
    [self RegistProperty:[[doProperty alloc]init:@"enabled" :Bool :@"false" :NO]];
    [self RegistProperty:[[doProperty alloc]init:@"source" :String :@"" :NO]];
    [self RegistProperty:[[doProperty alloc]init:@"scale" :String :@"" :NO]];
    [self RegistProperty:[[doProperty alloc]init:@"cacheType" :String :@"never" :NO]];
}

@end
