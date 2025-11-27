using UnityEngine;

public class GridTileView : MonoBehaviour
{
    
    public Vector2Int GridPosition;
    public float objectYOffset = 0.8f;
    void Start()
    {
        GridManager gridManager = GridManager.Instance;

        if (gridManager == null)
        {
            Debug.LogError("Sahnede GridManager bulunamadı!");
            return;
        }

        //// 2. Kendi dünya pozisyonunu (transform.position) grid koordinatına çevir.
        //// Elle yerleştirirken (0,0,0), (1,0,0) gibi tam sayılara koyduğun için
        //// bu yuvarlama işlemi mükemmel çalışacaktır.
        //GridPosition = new Vector2Int(
        //    Mathf.RoundToInt(transform.position.x),
        //    Mathf.RoundToInt(transform.position.z)
        //);

       
        gridManager.RegisterTile(this, GridPosition);
    }

   
    public Vector3 GetWorldPosition()
    {
        return new Vector3(
            transform.position.x,
            transform.position.y + objectYOffset, // Tile'ın Y'sine offset ekle
            transform.position.z
        );
    }
}