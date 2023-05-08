
Not compatible with URP and HDRP.

How to use:
Add the script called "SelectionOutlineController.cs" to the camera you want to affect.
Then play the demo scene, click and hold on any object in the scene and it will show the outline.

Parameters:

Selection Mode:		1,Only outline the single object you select.	2 ,outline its children too.
Outline Type:	1,Outline the whole object .	2,Outline the whole object but colorize the occluded parts.	3,Only outline the visible parts .
Alpha Mode:		1,Read the alpha data of MainTex and cause holes.		2,Only outline the intact geometry.
Outline Color: The outline Color.
Occluded Color: The color that the occluded parts will be tinted with.
Outline Width:	Outline's width.
Outline Hardness: How much the outline color blend in.


An object must be given a collider and the collider must be put at the same place where the its renderer is.So that it can be selected (By Raycast).

Selection codes are in the Update function of the script. Write your own codes there if you want.

Warning: 
Remember to  assign two related shaders: PostprocessOutline and Target, into the ProjectSettings->Graphics->Always Included Shaders.
Otherwise this function would not work properly after you built your game.


Advice:
It'd be better that the main texture property of the selected object was named as "_MainTex", and the alpha data are stored in its Alpha channel.
So the transparent and clipped objects can be outlined properly.

By BookSun

================================================================
本插件不与URP ,HDRP兼容
如何使用：
把本插件内的脚本 SelectionOutlineController.cs 添加到你要影响的相机上。
然后运行演示场景，鼠标点击并按住场景中任一物体，它会显示选中描边。
参数如下：

Selection Mode(选择模式):		1,只选中单个物体.	2 ,会把被选中物体的所有子物体都选中.
Outline Type(描边模式):	1,为整个物体描边 .	2,为整个物体描边但会把遮挡部分染色.	3,只为可见部分描边 .
Alpha Mode(透明模式):		1,读取主贴图的Alpha信息，描边上会因此产生孔洞.		2,只描边完整的几何体.
Outline Color(描边颜色): 描边的颜色
Occluded Color(遮挡颜色): 被遮挡部分的染色
Outline Width(描边宽度):	描边的宽度
Outline Hardness(描边硬度): 描边的软硬程度。

一个物体必须被添加碰撞体且碰撞体和Renderer在同一个位置，这样才能被选中并显示描边。（使用Raycast实现）
选中的逻辑控制代码都在脚本的Update函数内，若想修改请自便。

警告：
务必把相关的两个shaders:PostprocessOutline and Target 在项目设置中，设置为永远包含。 ProjectSettings->Graphics->Always Included Shaders.
否则你的游戏打包后 此功能会出现异常。

建议：
被选中物体的shader中，主贴图的Property最好被命名为"_MainTex"，并且贴图的Alpha信息存在A通道，这样，半透明物体和裁剪物体才能被正确描边。
