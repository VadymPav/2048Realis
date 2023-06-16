using Zenject;

public class GlobalInstaller : MonoInstaller<GlobalInstaller>
{
    public TileBoard board;
    public GameManager gameManager;
    public TileGrid tileGrid;
    public Tile tile;
    public override void InstallBindings()
    {
        Container.Bind<TileBoard>().FromInstance(board).AsSingle();
        Container.Bind<GameManager>().FromInstance(gameManager).AsSingle();
        Container.Bind<TileGrid>().FromInstance(tileGrid).AsSingle();
        Container.Bind<Tile>().FromInstance(tile).AsSingle();
    }
}