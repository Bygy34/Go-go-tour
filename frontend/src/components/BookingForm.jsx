import { useState } from 'react'

const API = 'http://localhost:5000/api'

export default function BookingForm({ tourId }) {
  const [form, setForm] = useState({ fullName: '', email: '', phone: '', message: '' })
  const [status, setStatus] = useState('')

  const onSubmit = async (e) => {
    e.preventDefault()
    setStatus('Отправляем...')

    const response = await fetch(`${API}/bookings/${tourId}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(form)
    })

    if (response.ok) {
      setStatus('Заявка отправлена!')
      setForm({ fullName: '', email: '', phone: '', message: '' })
    } else {
      setStatus('Ошибка отправки заявки')
    }
  }

  return (
    <form onSubmit={onSubmit} className="card form">
      <h3>Записаться на тур</h3>
      <input value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} placeholder="ФИО" required />
      <input value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} placeholder="Email" type="email" required />
      <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} placeholder="Телефон" required />
      <textarea value={form.message} onChange={(e) => setForm({ ...form, message: e.target.value })} placeholder="Комментарий" required />
      <button type="submit">Отправить</button>
      <small>{status}</small>
    </form>
  )
}
