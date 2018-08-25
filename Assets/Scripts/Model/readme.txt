author: Walker
github: https://github.com/liumingzw

1.need import Math.net

2.How to use Unity Math.net: http://answers.unity3d.com/questions/462042/unity-and-mathnet.html

3.Steps:

    1) Create a folder called Assets/Plugins in your project.

    2) go to  https://onedrive.live.com/?id=84F3672F8CDA3E91%21440210&cid=84F3672F8CDA3E91 and download the latest version of MatNet.Numerics.dll in zip format.

    3) open the folder called Net35. Unity apparently only runs on this version of .net.

    4) copy BOTH MathNet.Numerics.dll AND System.Threading.dll into Assets/Plugins.

Note: Don't modify anything in MonoDevelop. it should reference it automatically.

4.pure C# code to parse stl(binary&ascii) and obj file

5.todo: parse stl file, use Regex better