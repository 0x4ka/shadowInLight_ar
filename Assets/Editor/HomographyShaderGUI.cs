using UnityEditor;
using UnityEngine;

public class HomographyShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var material = materialEditor.target as Material;
        var bottomLeftTopLeft = FindProperty("_BottomLeftTopLeft", properties).vectorValue;
        var topRightBottomRight = FindProperty("_TopRightBottomRight", properties).vectorValue;
        EditorGUI.BeginChangeCheck();
        var bottomLeft = EditorGUILayout.Vector2Field(
            "Bottom Left",
            new Vector2(bottomLeftTopLeft.x, bottomLeftTopLeft.y));
        var topLeft = EditorGUILayout.Vector2Field(
            "Top Left",
            new Vector2(bottomLeftTopLeft.z, bottomLeftTopLeft.w));
        var topRight = EditorGUILayout.Vector2Field(
            "Top Right",
            new Vector2(topRightBottomRight.x, topRightBottomRight.y));
        var bottomRight = EditorGUILayout.Vector2Field(
            "Bottom Right",
            new Vector2(topRightBottomRight.z, topRightBottomRight.w));
        if (EditorGUI.EndChangeCheck())
        {
            material.SetVector(
                "_BottomLeftTopLeft",
                new Vector4(bottomLeft.x, bottomLeft.y, topLeft.x, topLeft.y));
            material.SetVector(
                "_TopRightBottomRight",
                new Vector4(topRight.x, topRight.y, bottomRight.x, bottomRight.y));
            var matrix = HomographyMatrix(bottomLeft, topLeft, topRight, bottomRight);
            material.SetVector("_HomographyMatrixR0", matrix.GetRow(0));
            material.SetVector("_HomographyMatrixR1", matrix.GetRow(1));
            material.SetVector("_HomographyMatrixR2", matrix.GetRow(2));
            material.SetVector("_HomographyMatrixR3", matrix.GetRow(3));
        }
        base.OnGUI(materialEditor, properties);
    }

    /// <summary>
    ///     Vector2の外積を求める
    /// </summary>
    /// <param name="a">左項</param>
    /// <param name="b">右項</param>
    /// <returns>外積</returns>
    private static float Cross(Vector2 a, Vector2 b)
    {
        return (a.x * b.y) - (b.x * a.y);
    }

    /// <summary>
    ///     4隅の座標を変換する行列を求める
    /// </summary>
    /// <param name="bl">(0, 0)の変換先</param>
    /// <param name="tl">(0, 1)の変換先</param>
    /// <param name="tr">(1, 1)の変換先</param>
    /// <param name="br">(1, 0)の変換先</param>
    /// <returns>変換行列</returns>
    private static Matrix4x4 HomographyMatrix(Vector2 bl, Vector2 tl, Vector2 tr, Vector2 br)
    {
        var cf = bl; // cとfはblの座標で確定
        Vector2 ghMatR0;
        Vector2 ghMatR1;
        Inverse(br - tr, tl - tr, out ghMatR0, out ghMatR1);
        var v = (bl + tr) - (tl + br);
        var gh = new Vector2(Vector2.Dot(ghMatR0, v), Vector2.Dot(ghMatR1, v)); // まずgとhを求める
        var ad = ((gh.x + 1.0f) * br) - bl; // gを使ってaとdを求める
        var be = ((gh.y + 1.0f) * tl) - bl; // hを使ってbとeを求める
        var result = Matrix4x4.identity; // Unityで扱うには4次元ベースの方が便利そうなので、Matrix4x4に整形しました
        result.SetColumn(0, new Vector4(ad.x, ad.y, 0.0f, gh.x));
        result.SetColumn(1, new Vector4(be.x, be.y, 0.0f, gh.y));
        result.SetColumn(2, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));
        result.SetColumn(3, new Vector4(cf.x, cf.y, 0.0f, 1.0f));
        return result;
    }

    /// <summary>
    ///     2x2行列の逆行列を求める
    /// </summary>
    /// <param name="column0">元の行列の0列目</param>
    /// <param name="column1">元の行列の1列目</param>
    /// <param name="inverseRow0">逆行列の0行目</param>
    /// <param name="inverseRow1">逆行列の1行目</param>
    private static void Inverse(Vector2 column0, Vector2 column1, out Vector2 inverseRow0, out Vector2 inverseRow1)
    {
        var determinant = Cross(column0, column1);
        inverseRow0 = new Vector2(column1.y, -column1.x) / determinant;
        inverseRow1 = new Vector2(-column0.y, column0.x) / determinant;
    }
}