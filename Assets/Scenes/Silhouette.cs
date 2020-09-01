using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer), typeof(PolygonCollider2D))]
public class Silhouette : MonoBehaviour
{
    private static readonly int SaturationThresholdProperty = Shader.PropertyToID("_SaturationThreshold");
    private static readonly int ValueThresholdProperty = Shader.PropertyToID("_ValueThreshold");
    private static readonly int HomographyMatrixProperty = Shader.PropertyToID("_HomographyMatrix");

    [SerializeField] private int width = 160*2;
    [SerializeField] private int height = 90*2;
    [SerializeField] private int floorHeight = 16;
    [SerializeField] private int resolutionMultiplier = 1;
    [SerializeField] private Color color = Color.red;

    public Slider scaleW;
    public Slider scaleH;
    public Slider BottomLeftX;
    public Slider BottomLeftY;
    public Slider TopLeftX;
    public Slider TopLeftY;
    public Slider TopRightX;
    public Slider TopRightY;
    public Slider BottomRightX;
    public Slider BottomRightY;
    public Slider ResetHomography;

    public Slider SaturationThreshold_slider;
    public Slider ValueThreshold_slider;

    public Toggle color_shilouette;
    public Toggle normalorwh;
    int i;


    // 彩度のしきい値
    // これよりも彩度が高ければ除外する
    [SerializeField] [Range(0.0f, 1.0f)] public float saturationThreshold = 0.25f;

    // 明度のしきい値
    // これよりも明度が高ければ除外する
    [SerializeField] [Range(0.0f, 1.0f)] public float valueThreshold = 0.25f;

    [SerializeField] public Shader silhouetteShader;


    // 四隅の位置を指定するためのフィールドを追加
    [Header("Corners")]
    [SerializeField] public Vector2 bottomLeft = new Vector2(0.000f, 0.000f);
    [SerializeField] public Vector2 topLeft = new Vector2(0.000f, 1.000f);
    [SerializeField] public Vector2 topRight = new Vector2(1.000f, 1.000f);
    [SerializeField] public Vector2 bottomRight = new Vector2(1.000f, 1.000f);

    Vector2 BL = new Vector2(0.000f, 0.000f);
    Vector2 TL = new Vector2(0.000f, 1.000f);
    Vector2 TR = new Vector2(1.000f, 1.000f);
    Vector2 BR = new Vector2(1.000f, 1.000f);



    private new SpriteRenderer renderer;
    private new PolygonCollider2D collider;
    private Sprite sprite;
    private Texture2D spriteTexture;
    private Material silhouetteMaterial;
    private WebCamTexture webCamTexture;
    public RawImage rawImage;

    // 以前作成したHomographyShaderGUIのものと同様の変換行列作成メソッドを定義
    private static Matrix4x4 HomographyMatrix(Vector2 bl, Vector2 tl, Vector2 tr, Vector2 br)
    {
        var cf = bl;
        Inverse(br - tr, tl - tr, out var ghMatR0, out var ghMatR1);
        var v = (bl + tr) - (tl + br);
        var gh = new Vector2(Vector2.Dot(ghMatR0, v), Vector2.Dot(ghMatR1, v));
        var ad = ((gh.x + 1.0f) * br) - bl;
        var be = ((gh.y + 1.0f) * tl) - bl;
        var result = Matrix4x4.identity;
        result.SetColumn(0, new Vector4(ad.x, ad.y, 0.0f, gh.x));
        result.SetColumn(1, new Vector4(be.x, be.y, 0.0f, gh.y));
        result.SetColumn(2, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));
        result.SetColumn(3, new Vector4(cf.x, cf.y, 0.0f, 1.0f));
        return result;
    }

    private static void Inverse(Vector2 column0, Vector2 column1, out Vector2 inverseRow0, out Vector2 inverseRow1)
    {
        var determinant = Cross(column0, column1);
        inverseRow0 = new Vector2(column1.y, -column1.x) / determinant;
        inverseRow1 = new Vector2(-column0.y, column0.x) / determinant;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return (a.x * b.y) - (b.x * a.y);
    }


    private IEnumerator Start()
    {
        scaleW = scaleW.GetComponent<Slider>();
        scaleH = scaleH.GetComponent<Slider>();
        BottomLeftX = BottomLeftX.GetComponent<Slider>();
        BottomLeftY = BottomLeftY.GetComponent<Slider>();
        TopLeftX = TopLeftX.GetComponent<Slider>();
        TopLeftY = TopLeftY.GetComponent<Slider>();
        TopRightX = TopRightX.GetComponent<Slider>();
        TopRightY = TopRightY.GetComponent<Slider>();
        BottomRightX = BottomRightX.GetComponent<Slider>();
        BottomRightY = BottomRightY.GetComponent<Slider>();
        ResetHomography = ResetHomography.GetComponent<Slider>();

        color_shilouette = color_shilouette.GetComponent<Toggle>();
        normalorwh = normalorwh.GetComponent<Toggle>();

        if (WebCamTexture.devices.Length <= 0)  //webcamの接続個数が0以下なら修了
        {
            Destroy(this);
            yield break;
        }

        this.webCamTexture = new WebCamTexture(WebCamTexture.devices[WebCamTexture.devices.Length-1].name, this.width, this.height, 30);
        rawImage.texture = webCamTexture;
        this.webCamTexture.Play();  //カメラ再生

       

        this.width = this.webCamTexture.width * this.resolutionMultiplier;  //ここで拡大縮小をするが処理が重くなる
        this.height = this.webCamTexture.height * this.resolutionMultiplier;    //ダイアグラムではスルーする

        var pixelsPerUnit = (float)this.height;
        var mainCamera = Camera.main;
        if (mainCamera.orthographic)
        {
            pixelsPerUnit = (0.5f * Screen.height * this.resolutionMultiplier) / mainCamera.orthographicSize;
        }

        this.renderer = this.GetComponent<SpriteRenderer>();
        this.collider = this.GetComponent<PolygonCollider2D>();
        this.spriteTexture = new Texture2D(this.width, this.height, TextureFormat.ARGB32, false);
        this.sprite = Sprite.Create(
            this.spriteTexture,
            new Rect(0, 0, this.width, this.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);
        this.renderer.sprite = this.sprite;
        this.silhouetteMaterial = new Material(this.silhouetteShader);


        // 下のwhileループ中でマテリアルのプロパティをセットする方式ならば、プレイモード中に
        // インスペクタ上の値を書き換えるとそれが反映されるため調整が容易かと思いますが
        // (ただしコライダーを常時付け外ししているため、再生状態のままでは書き換えが
        // 困難かと思います...一時停止ボタンで止めた状態で書き換える必要があるでしょう)、
        // あらかた調整が済んだら、マテリアルへのプロパティ設定をwhileループの外に出してやれば
        // ループ内でいちいちプロパティ再設定が行われなくなるため、実行する上での効率が少しだけ
        // よくなるかと思います。
        /*
        this.silhouetteMaterial.color = this.color;
        this.silhouetteMaterial.SetFloat(SaturationThresholdProperty, this.saturationThreshold);
        this.silhouetteMaterial.SetFloat(ValueThresholdProperty, this.valueThreshold);

        // 四隅の座標からUV変換行列を作成してマテリアルにセットする
        this.silhouetteMaterial.SetMatrix(HomographyMatrixProperty, HomographyMatrix(
            this.bottomLeft,
            this.topLeft,
            this.topRight,
            this.bottomRight));
        */



        while (true)
        {
            this.silhouetteMaterial.color = this.color;
            this.silhouetteMaterial.SetFloat(SaturationThresholdProperty, this.saturationThreshold);
            this.silhouetteMaterial.SetFloat(ValueThresholdProperty, this.valueThreshold);

            // 四隅の座標からUV変換行列を作成してマテリアルにセットする
            this.silhouetteMaterial.SetMatrix(HomographyMatrixProperty, HomographyMatrix(
                this.bottomLeft,
                this.topLeft,
                this.topRight,
                this.bottomRight));

            var renderTexture = RenderTexture.GetTemporary(this.width, this.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(this.webCamTexture, renderTexture, this.silhouetteMaterial, 0);
            var activeTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;

            var floorY = ((float)(this.floorHeight * this.resolutionMultiplier) / this.height);
            GL.PushMatrix();
            this.silhouetteMaterial.SetPass(1);
            GL.Begin(GL.QUADS);
            GL.LoadOrtho();
            GL.Color(this.color);
            GL.Vertex(Vector3.zero);
            GL.Vertex3(0.0f, floorY, 0.0f);
            GL.Vertex3(1.0f, floorY, 0.0f);
            GL.Vertex(Vector3.right);
            GL.End();
            GL.PopMatrix();

            this.spriteTexture.ReadPixels(new Rect(0, 0, this.width, this.height), 0, 0);
            RenderTexture.active = activeTexture;
            RenderTexture.ReleaseTemporary(renderTexture);
            this.spriteTexture.Apply();

            Destroy(this.collider);
            this.collider = this.gameObject.AddComponent<PolygonCollider2D>();
            yield return null;
        }
    }

    void Update()
    {
        saturationThreshold = SaturationThreshold_slider.GetComponent<Slider>().value;
        valueThreshold = ValueThreshold_slider.GetComponent<Slider>().value;

        if(normalorwh.isOn == false)
        {
            this.bottomLeft = new Vector2(0 - scaleW.value, 0 - scaleH.value);
            this.topLeft = new Vector2(0 + scaleW.value, 1 + scaleH.value);
            this.topRight = new Vector2(1 - scaleW.value, 1 - scaleH.value);
            this.bottomRight = new Vector2(1 + scaleW.value, 0 + scaleH.value);


            if (ResetHomography.value == 1)
            {
                scaleW.value = 0;
                scaleH.value = 0;
            }
        }
        else
        {
            this.bottomLeft = new Vector2(BottomLeftX.value, BottomLeftY.value);
            this.topLeft = new Vector2(TopLeftX.value, TopLeftY.value);
            this.topRight = new Vector2(TopRightX.value, TopRightY.value);
            this.bottomRight = new Vector2(BottomRightX.value, BottomRightY.value);

            if (ResetHomography.value == 1)
            {
                BottomLeftX.value = 0;
                BottomLeftY.value = 0;
                TopLeftX.value = 0;
                TopLeftY.value = 1;
                TopRightX.value = 1;
                TopRightY.value = 1;
                BottomRightX.value = 1;
                BottomRightY.value = 0;
            }
        }


        if (color_shilouette.isOn == true)
        {
            this.color = Color.red;
        }
        else
        {
            this.color = Color.white;
        }
    }

}

/*
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(PolygonCollider2D))]
public class Silhouette : MonoBehaviour
{
    private static readonly int Threshold = Shader.PropertyToID("_Threshold");

    [SerializeField] private int width = 1280;
    [SerializeField] private int height = 720;
    [SerializeField] private int floorHeight = 1;
    [SerializeField] private int resolutionMultiplier = 1;
    [SerializeField] private Color color = Color.black;
    [SerializeField] [Range(0.00f, 1.00f)] private float threshold = 0.50f;
    [SerializeField] private Shader silhouetteShader;
    int i = 0;

    private new SpriteRenderer renderer;
    private new PolygonCollider2D collider;
    private Sprite sprite;
    private Texture2D spriteTexture;
    private Material silhouetteMaterial;
    private WebCamTexture webCamTexture;
    private WebCamTexture webCamTexture2;

    private IEnumerator Start()
    {
        // WebCamTextureをセットアップ
        if (WebCamTexture.devices.Length <= 0)
        {
            Destroy(this);
            yield break;
        }
        this.width = Mathf.Max(this.width, 100);
        this.height = Mathf.Max(this.height, 100);
        this.webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name, this.width, this.height, 30);
        this.webCamTexture.Play();
        while (this.webCamTexture.width < 100)
        {
            yield return null;
        }

        this.webCamTexture2 = webCamTexture;
        
        // この後の処理で使うテクスチャ幅・高さを求める
        // resolutionMultiplierを大きくするとコライダーが精密になるが、負荷は大きくなる
        this.width = this.webCamTexture.width * this.resolutionMultiplier;
        this.height = this.webCamTexture.height * this.resolutionMultiplier;
        var pixelsPerUnit = (float)this.height;
        var mainCamera = Camera.main;
        if (mainCamera.orthographic)
        {
            pixelsPerUnit = (0.5f * Screen.height * this.resolutionMultiplier) / mainCamera.orthographicSize;
        }

        // スプライトをセットアップ
        this.renderer = this.GetComponent<SpriteRenderer>();
        this.collider = this.GetComponent<PolygonCollider2D>();
        this.spriteTexture = new Texture2D(this.width, this.height, TextureFormat.ARGB32, false);
        this.sprite = Sprite.Create(
            this.spriteTexture,
            new Rect(0, 0, this.width, this.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);
        this.renderer.sprite = this.sprite;

        this.silhouetteMaterial = new Material(this.silhouetteShader);
        while (true)
        {
            // WebCamTextureの内容を2値化
            this.silhouetteMaterial.color = this.color;
            this.silhouetteMaterial.SetFloat(Threshold, this.threshold);
            var renderTexture = RenderTexture.GetTemporary(this.width, this.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(this.webCamTexture, renderTexture, this.silhouetteMaterial, 0);
            var activeTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;

            // 床を描く
            // WebCamTextureのBlitで何も映っていない場合、後のコライダー生成時に
            // デフォルトの形状で生成されてしまうため、それを防止するのが主目的
            var floorY = ((float)(this.floorHeight * this.resolutionMultiplier) / this.height);
            GL.PushMatrix();
            this.silhouetteMaterial.SetPass(1);
            GL.Begin(GL.QUADS);
            GL.LoadOrtho();
            GL.Color(this.color);
            GL.Vertex(Vector3.zero);
            GL.Vertex3(0.0f, floorY, 0.0f);
            GL.Vertex3(1.0f, floorY, 0.0f);
            GL.Vertex(Vector3.right);
            GL.End();
            GL.PopMatrix();

            // 結果をスプライト用テクスチャに読み取る
            this.spriteTexture.ReadPixels(new Rect(0, 0, this.width, this.height), 0, 0);
            RenderTexture.active = activeTexture;
            RenderTexture.ReleaseTemporary(renderTexture);
            this.spriteTexture.Apply();

            // PolygonCollider2Dを再作成し、不透明部分に沿った形状を作らせる
            Destroy(this.collider);
            this.collider = this.gameObject.AddComponent<PolygonCollider2D>();
            yield return null;
        }
    }
    */
