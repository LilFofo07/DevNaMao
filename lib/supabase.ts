import { createClient } from "@supabase/supabase-js"

// Crie um cliente Supabase com suas credenciais
export const supabase = createClient(
  process.env.NEXT_PUBLIC_SUPABASE_URL || "",
  process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY || "",
)

export interface MenuItem {
  id: string
  name: string
  description: string
  price: number
  imageUrl: string
  created_at?: string
}

// Funções para interagir com o banco de dados
export async function getMenuItems(): Promise<MenuItem[]> {
  const { data, error } = await supabase.from("menu_items").select("*").order("created_at", { ascending: false })

  if (error) {
    console.error("Erro ao buscar itens do menu:", error)
    return []
  }

  return data || []
}

export async function addMenuItem(item: Omit<MenuItem, "id" | "created_at">): Promise<MenuItem | null> {
  const { data, error } = await supabase.from("menu_items").insert([item]).select()

  if (error) {
    console.error("Erro ao adicionar item ao menu:", error)
    return null
  }

  return data?.[0] || null
}

export async function removeMenuItem(id: string): Promise<boolean> {
  const { error } = await supabase.from("menu_items").delete().eq("id", id)

  if (error) {
    console.error("Erro ao remover item do menu:", error)
    return false
  }

  return true
}
