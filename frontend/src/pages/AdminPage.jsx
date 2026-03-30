import { useEffect, useState } from 'react'

const API = 'http://localhost:5000/api'

const defaultTour = {
  title: '',
  country: '',
  description: '',
  price: 0,
  durationDays: 1,
  isActive: true
}

export default function AdminPage() {
  const [token, setToken] = useState('super-admin-token')
  const [tours, setTours] = useState([])
  const [bookings, setBookings] = useState([])
  const [tourForm, setTourForm] = useState(defaultTour)
  const [editId, setEditId] = useState(null)

  const headers = { 'Content-Type': 'application/json', 'X-Admin-Token': token }

  const loadData = async () => {
    const [toursRes, bookingsRes] = await Promise.all([
      fetch(`${API}/tours/admin`, { headers }),
      fetch(`${API}/bookings/admin`, { headers })
    ])

    if (toursRes.ok) setTours(await toursRes.json())
    if (bookingsRes.ok) setBookings(await bookingsRes.json())
  }

  useEffect(() => {
    loadData()
  }, [])

  const submitTour = async (e) => {
    e.preventDefault()
    const url = editId ? `${API}/tours/admin/${editId}` : `${API}/tours/admin`
    const method = editId ? 'PUT' : 'POST'

    await fetch(url, {
      method,
      headers,
      body: JSON.stringify(tourForm)
    })

    setTourForm(defaultTour)
    setEditId(null)
    await loadData()
  }

  const startEdit = (tour) => {
    setEditId(tour.id)
    setTourForm({ ...tour, isActive: true })
  }

  return (
    <section>
      <h2>Админ панель</h2>
      <label>
        Admin token:
        <input value={token} onChange={(e) => setToken(e.target.value)} />
      </label>
      <button onClick={loadData}>Обновить данные</button>

      <form onSubmit={submitTour} className="card form">
        <h3>{editId ? 'Редактирование тура' : 'Создание тура'}</h3>
        <input placeholder="Название" value={tourForm.title} onChange={(e) => setTourForm({ ...tourForm, title: e.target.value })} required />
        <input placeholder="Страна" value={tourForm.country} onChange={(e) => setTourForm({ ...tourForm, country: e.target.value })} required />
        <textarea placeholder="Описание" value={tourForm.description} onChange={(e) => setTourForm({ ...tourForm, description: e.target.value })} required />
        <input type="number" placeholder="Цена" value={tourForm.price} onChange={(e) => setTourForm({ ...tourForm, price: Number(e.target.value) })} required />
        <input type="number" placeholder="Длительность" value={tourForm.durationDays} onChange={(e) => setTourForm({ ...tourForm, durationDays: Number(e.target.value) })} required />
        <button type="submit">Сохранить</button>
      </form>

      <h3>Туры</h3>
      <div className="grid">
        {tours.map((tour) => (
          <article key={tour.id} className="card">
            <h4>{tour.title}</h4>
            <p>{tour.country}</p>
            <p>{tour.price} €</p>
            <button onClick={() => startEdit(tour)}>Редактировать</button>
          </article>
        ))}
      </div>

      <h3>Заявки</h3>
      <div className="grid">
        {bookings.map((booking) => (
          <article key={booking.id} className="card">
            <h4>{booking.tourTitle}</h4>
            <p>{booking.fullName}</p>
            <p>{booking.email}</p>
            <p>{booking.phone}</p>
            <p>{booking.message}</p>
          </article>
        ))}
      </div>
    </section>
  )
}
