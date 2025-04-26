import { getMenuItems } from "@/lib/supabase"
import { Card, CardContent } from "@/components/ui/card"
import Image from "next/image"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { ArrowLeft } from "lucide-react"

export const revalidate = 0 // Desativa o cache para sempre mostrar dados atualizados

export default async function CardapioPage() {
  const menuItems = await getMenuItems()

  return (
    <div className="flex min-h-screen flex-col">
      <header className="bg-white border-b sticky top-0 z-10">
        <div className="container flex h-16 items-center px-4 md:px-6">
          <Link href="/" className="mr-4">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-5 w-5" />
              <span className="sr-only">Voltar</span>
            </Button>
          </Link>
          <h1 className="text-xl font-bold">Cardápio - Restaurante Delícia</h1>
        </div>
      </header>
      <main className="flex-1 py-8">
        <div className="container px-4 md:px-6">
          {menuItems.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-lg text-gray-500">Nenhum item no cardápio ainda.</p>
            </div>
          ) : (
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {menuItems.map((item) => (
                <Card key={item.id} className="overflow-hidden">
                  <div className="aspect-video relative">
                    <Image src={item.imageUrl || "/placeholder.svg"} alt={item.name} fill className="object-cover" />
                  </div>
                  <CardContent className="p-4">
                    <div className="flex justify-between items-start mb-2">
                      <h3 className="text-lg font-bold">{item.name}</h3>
                      <span className="font-bold text-green-600">R$ {item.price.toFixed(2)}</span>
                    </div>
                    <p className="text-sm text-gray-500">{item.description}</p>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </div>
      </main>
      <footer className="border-t py-6">
        <div className="container px-4 md:px-6">
          <p className="text-center text-sm text-gray-500">© 2024 Restaurante Delícia. Todos os direitos reservados.</p>
        </div>
      </footer>
    </div>
  )
}
