# TShockPlayerManager

查看玩家背包、导出玩家数据，背包快照。


# 部分指令
```
/playermanager help, 玩家管理 帮助
/pm help, 同上一条

/pm look <玩家名>, 查看玩家

/pm export [玩家名], 导出单个玩家存档
/pm exportall, 导出全部玩家存档

/pm hp <玩家名>, 修改生命值
/pm maxhp <玩家名> <生命上限>, 修改生命上限
/pm mana <玩家名>, 修改魔力值
/pm maxmana <玩家名> <魔力上限>, 修改魔力上限


/bagsnapshot help, 背包快照 帮助
/bs help, 同上一条
/bs add <玩家名>, 添加背包快照
/bs del <玩家名>, 删除背包快照
/bs look <玩家名>, 查看背包快照
/bs recover <玩家名>, 恢复背包快照

/bs addall, 添加背包快照-全部在线玩家
/bs aa, 同上一条
/bs recoverall, 添加背包快照-全部在线玩家
/bs ra, 同上一条

/savemybag, 为自己创建背包快照
/smb, 同上一条
/lookmybag, 查看自己创建的背包快照
/lmb, 同上一条
```


# 说明
导出玩家数据，使用了此项目的部分代码： 
https://github.com/Megghy/PlayerExport