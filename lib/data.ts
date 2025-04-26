// Simulação de um banco de dados em memória

export interface MenuItem {
  id: string
  name: string
  description: string
  price: number
  imageUrl: string
}

// Dados iniciais
export let MenuItems: MenuItem[] = [
  {
    id: "1",
    name: "X-Burger Especial",
    description: "Hambúrguer artesanal, queijo cheddar, alface, tomate e molho especial da casa.",
    price: 25.9,
    imageUrl: "/placeholder.svg?height=300&width=400",
  },
  {
    id: "2",
    name: "Batata Frita Supreme",
    description: "Porção de batatas fritas crocantes com queijo, bacon e molho ranch.",
    price: 18.5,
    imageUrl: "/placeholder.svg?height=300&width=400",
  },
  {
    id: "3",
    name: "Milk Shake de Chocolate",
    description: "Milk shake cremoso de chocolate com calda e chantilly.",
    price: 15.9,
    imageUrl: "/placeholder.svg?height=300&width=400",
  },
  {
    id: "4",
    name: "Combo Família",
    description: "4 hambúrgueres, 2 porções de batata e 4 refrigerantes.",
    price: 89.9,
    imageUrl: "/placeholder.svg?height=300&width=400",
  },
  {
    id: "5",
    name: "Salada Caesar",
    description: "Alface, croutons, frango grelhado, queijo parmesão e molho caesar.",
    price: 22.9,
    imageUrl: "/placeholder.svg?height=300&width=400",
  },
  {
    id: "6",
    name: "Refrigerante",
    description: "Lata 350ml (Coca-Cola, Guaraná, Sprite ou Fanta).",
    price: 6.5,
    imageUrl: "/placeholder.svg?height=300&width=400",
  },
]

// Função para adicionar um novo item
export function addMenuItem(item: MenuItem) {
  MenuItems.push(item)
  return item
}

// Função para remover um item
export function removeMenuItem(id: string) {
  MenuItems = MenuItems.filter((item) => item.id !== id)
}

// Função para atualizar um item
export function updateMenuItem(id: string, updatedItem: Partial<MenuItem>) {
  MenuItems = MenuItems.map((item) => {
    if (item.id === id) {
      return { ...item, ...updatedItem }
    }
    return item
  })
}
