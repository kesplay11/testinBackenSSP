 public async Task<List<CLIContenedorItemsRecepcionBloqDTO>> GetAllPlaquesForSectorsAndForModels()
 {
     try
     {
         var datosBrutos = await _entity.Where((e) => e.Deleted != true)
             .Include((e) => e.CLIContenedorItems).ThenInclude((e) => e.CLISectores)
             .ToListAsync();

         var sectores = new[] { "IM", "PROD", "CLI" };

         var data = datosBrutos
             // Agrupamos primero por modelo para tener una lista plana de modelos
             .GroupBy(e => e.CLIContenedorItems.Modelo)
             .Select(g => new
             {
                 Modelo = g.Key,
                 // Agrupamos dentro de cada modelo por sector para calcular las sumas
                 SectorQuantities = g.GroupBy(s => s.CLIContenedorItems.CLISectores.NombreSector)
                                     .ToDictionary(s => s.Key, s => s.Sum(x => x.CLIContenedorItems.CantidadTotalItems))
             })
             // Proyectamos a la estructura final
             .Select(x => new CLIContenedorItemsRecepcionBloqDTO
             {
                 Modelo = x.Modelo,
                 // Usamos TryGetValue para evitar errores si un modelo no tiene datos en un sector
                 IM = x.SectorQuantities.TryGetValue("IM", out var imCount) ? imCount : 0,
                 PROD = x.SectorQuantities.TryGetValue("PROD", out var prodCount) ? prodCount : 0,
                 CLI = x.SectorQuantities.TryGetValue("CLI", out var cliCount) ? cliCount : 0,
             })
             .ToList();

         return data;
     }
     catch (Exception ex)
     {
         throw new Exception(ex.Message);
     }
 }