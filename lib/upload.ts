import { supabase } from "./supabase"

export async function uploadImage(file: File): Promise<string> {
  try {
    // Cria um nome único para o arquivo
    const fileExt = file.name.split(".").pop()
    const fileName = `${Math.random().toString(36).substring(2, 15)}_${Date.now()}.${fileExt}`
    const filePath = `menu-items/${fileName}`

    // Faz o upload do arquivo para o Storage do Supabase
    const { error: uploadError } = await supabase.storage.from("images").upload(filePath, file)

    if (uploadError) {
      throw uploadError
    }

    // Obtém a URL pública do arquivo
    const { data } = supabase.storage.from("images").getPublicUrl(filePath)

    return data.publicUrl
  } catch (error) {
    console.error("Erro ao fazer upload da imagem:", error)
    throw error
  }
}
