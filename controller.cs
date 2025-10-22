 [HttpGet("GetAllPlaquesForSectorsAndForModels")]
        public async Task<ActionResult<List<CLIContenedorItemsRecepcionBloqDTO>>> GetAllPlaquesForSectorsAndForModels()
        {
            try
            {
                var data = await _repository.GetAllPlaquesForSectorsAndForModels();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }