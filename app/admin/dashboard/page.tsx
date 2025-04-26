"use client"

import type React from "react"

import { useState } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { MenuItems, addMenuItem } from "@/lib/data"
import { ArrowLeft, LogOut, Plus, Trash } from "lucide-react"
import Image from "next/image"

export default function DashboardPage() {
  const router = useRouter()
  const [name, setName] = useState("")
  const [description, setDescription] = useState("")
  const [price, setPrice] = useState("")
  const [imageUrl, setImageUrl] = useState("")
  const [items, setItems] = useState(MenuItems)

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()

    if (!name || !description || !price || !imageUrl) {
      alert("Por favor, preencha todos os campos")
      return
    }

    const newItem = {
      id: Date.now().toString(),
      name,
      description,
      price: Number.parseFloat(price),
      imageUrl,
    }

    // Adiciona o item ao estado local e ao "banco de dados" simulado
    addMenuItem(newItem)
    setItems([...items, newItem])

    // Limpa o formulário
    setName("")
    setDescription("")
    setPrice("")
    setImageUrl("")
  }

  const handleLogout = () => {
    router.push("/")
  }

  const handleDelete = (id: string) => {
    // Remove o item do estado local
    const updatedItems = items.filter((item) => item.id !== id)
    setItems(updatedItems)

    // Em uma aplicação real, você também removeria do banco de dados
  }

  return (
    <div className="flex min-h-screen flex-col">
      <header className="bg-white border-b sticky top-0 z-10">
        <div className="container flex h-16 items-center justify-between px-4 md:px-6">
          <div className="flex items-center gap-4">
            <Link href="/">
              <Button variant="ghost" size="icon">
                <ArrowLeft className="h-5 w-5" />
                <span className="sr-only">Voltar</span>
              </Button>
            </Link>
            <h1 className="text-xl font-bold">Painel Administrativo</h1>
          </div>
          <Button variant="ghost" size="sm" onClick={handleLogout} className="gap-1">
            <LogOut className="h-4 w-4" />
            Sair
          </Button>
        </div>
      </header>
      <main className="flex-1 py-8">
        <div className="container px-4 md:px-6">
          <div className="grid gap-8 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Plus className="h-5 w-5" />
                  Adicionar Novo Item
                </CardTitle>
                <CardDescription>Preencha os detalhes para adicionar um novo item ao cardápio</CardDescription>
              </CardHeader>
              <CardContent>
                <form onSubmit={handleSubmit} className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="name">Nome do Lanche</Label>
                    <Input
                      id="name"
                      value={name}
                      onChange={(e) => setName(e.target.value)}
                      placeholder="Ex: X-Tudo"
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="description">Descrição</Label>
                    <Textarea
                      id="description"
                      value={description}
                      onChange={(e) => setDescription(e.target.value)}
                      placeholder="Ex: Hambúrguer com queijo, alface, tomate e molho especial"
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="price">Preço (R$)</Label>
                    <Input
                      id="price"
                      type="number"
                      step="0.01"
                      min="0"
                      value={price}
                      onChange={(e) => setPrice(e.target.value)}
                      placeholder="Ex: 25.90"
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="imageUrl">URL da Imagem</Label>
                    <Input
                      id="imageUrl"
                      value={imageUrl}
                      onChange={(e) => setImageUrl(e.target.value)}
                      placeholder="https://exemplo.com/imagem.jpg"
                      required
                    />
                    <p className="text-xs text-gray-500">Dica: Use imagens do Unsplash ou similar para testes</p>
                  </div>
                  <Button type="submit" className="w-full">
                    Adicionar Item
                  </Button>
                </form>
              </CardContent>
            </Card>

            <div className="space-y-6">
              <h2 className="text-xl font-bold">Itens do Cardápio</h2>
              <div className="space-y-4">
                {items.length === 0 ? (
                  <p className="text-gray-500">Nenhum item no cardápio ainda.</p>
                ) : (
                  items.map((item) => (
                    <Card key={item.id} className="overflow-hidden">
                      <div className="flex">
                        <div className="w-24 h-24 relative">
                          <Image
                            src={item.imageUrl || "/placeholder.svg"}
                            alt={item.name}
                            fill
                            className="object-cover"
                          />
                        </div>
                        <CardContent className="flex-1 p-4">
                          <div className="flex justify-between items-start">
                            <div>
                              <h3 className="font-bold">{item.name}</h3>
                              <p className="text-sm text-gray-500 line-clamp-2">{item.description}</p>
                              <p className="text-sm font-medium text-green-600 mt-1">R$ {item.price.toFixed(2)}</p>
                            </div>
                            <Button
                              variant="ghost"
                              size="icon"
                              className="text-red-500 hover:text-red-700 hover:bg-red-50"
                              onClick={() => handleDelete(item.id)}
                            >
                              <Trash className="h-4 w-4" />
                              <span className="sr-only">Excluir</span>
                            </Button>
                          </div>
                        </CardContent>
                      </div>
                    </Card>
                  ))
                )}
              </div>
            </div>
          </div>
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
