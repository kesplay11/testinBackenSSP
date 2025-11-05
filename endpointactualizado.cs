blic async Task<List<CLIContenedorItemsRecepcionBloqDTO>> GetAllPlaquesForSectorsAndForModels()
{
    try
    {
        // Traemos los datos brutos con las inclusiones necesarias
        var datosBrutos = await _entity.Where((e) => e.Deleted != true)
            .Include((e) => e.CLIContenedorItems).ThenInclude((e) => e.CLISectores)
            .ToListAsync();

        // El array 'sectores' ya no se usa, lo podemos omitir
        // var sectores = new[] { "IM", "PROD", "CLI" }; 

        var data = datosBrutos
            // 1. Agrupamos por modelo
            .GroupBy(e => e.CLIContenedorItems.Modelo)
            
            // 2. ⚡️ ORDENACIÓN: Ordenamos por la FECHA DE MODIFICACIÓN MÁXIMA dentro del grupo.
            //    Esto asegura que el modelo con el cambio más reciente esté primero (índice 0).
            .OrderByDescending(g => g.Max(e => e.LastModifiedDate)) // <-- ¡CAMBIO CLAVE AQUÍ!
            
            // 3. Proyectamos a la estructura anónima
            .Select(g => new
            {
                Modelo = g.Key,
                // Agrupamos dentro de cada modelo por sector para calcular las sumas
                SectorQuantities = g.GroupBy(s => s.CLIContenedorItems.CLISectores.NombreSector)
                                    .ToDictionary(s => s.Key, s => s.Sum(x => x.CLIContenedorItems.CantidadTotalItems))
            })
            // 4. Proyectamos a la estructura final (DTO)
            .Select(x =>
            {
                // 1. Lógica para IM
                var imKey = x.SectorQuantities.Keys.FirstOrDefault(key => key.Contains("IM"));
                int imCount = 0;
                if (imKey != null && x.SectorQuantities.TryGetValue(imKey, out var foundImCount))
                {
                    imCount = foundImCount;
                }

                // 2. Lógica para PROD
                var prodKey = x.SectorQuantities.Keys.FirstOrDefault(key => key.Contains("Prod"));
                int prodCount = 0;
                if (prodKey != null && x.SectorQuantities.TryGetValue(prodKey, out var foundProdCount))
                {
                    prodCount = foundProdCount;
                }

                // 3. Lógica para CLI
                var cliKey = x.SectorQuantities.Keys.FirstOrDefault(key => key.Contains("CLI"));
                int cliCount = 0;
                if (cliKey != null && x.SectorQuantities.TryGetValue(cliKey, out var foundCliCount))
                {
                    cliCount = foundCliCount;
                }

                // Devolvemos la estructura final (DTO)
                return new CLIContenedorItemsRecepcionBloqDTO
                {
                    Modelo = x.Modelo,
                    IM = imCount,
                    PROD = prodCount,
                    CLI = cliCount,
                };
            })
            .ToList();

        return data;
    }
    catch (Exception ex)
    {
        // Es mejor lanzar una excepción más específica o loggear el error.
        throw new Exception("Error al obtener y ordenar las placas por última modificación: " + ex.Message);
    }
}