# TShockPlayerManager

导出/备份/恢复玩家存档，查看玩家背包。


# 部分指令
```
/pm help, 帮助

/pm look <玩家名>, 查看玩家
/lookbag <玩家名>, 查看玩家（普通用户）

/pm export <玩家名>, 导出单个玩家存档
/pm exportall, 导出全部玩家存档

/pm hp <玩家名> <生命值>, 修改生命值
/pm maxhp <玩家名> <生命上限>, 修改生命上限
/pm mana <玩家名> <魔力值>, 修改魔力值
/pm maxmana <玩家名> <魔力上限>, 修改魔力上限
/pm quest <玩家名> <次数>, 修改钓鱼任务次数（v1.3）
/pm enhance help, 开启/关闭 永久增强属性（v1.3）

/pm backup help, 备份（v1.3）
/pm recover help, 恢复（v1.3）
/pm list help, 列表（v1.3）
/pm reload, 重载配置（v1.3）

// 主指令的其它写法
/pm = /playermanager
.pm = /pm

// 简写
/pm look = /pm l
/lookbag = /lb
/pm export = /pm e
/pm exportall = /pm ea
/pm maxhp = /pm mh
/pm maxmana = /pm mm
/pm enhance = /pm en
/pm backup = /pm b
/pm recover = /pm r
/pm list = /pm ls
```

# 权限
```
playermanager
lookbag
```

开放查背包功能
```
/group addperm default lookbag
```


# 说明
导出玩家数据部分，使用了此项目的部分代码： 
https://github.com/Megghy/PlayerExport